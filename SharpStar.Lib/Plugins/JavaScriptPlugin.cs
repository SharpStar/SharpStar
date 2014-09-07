// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using Jint;
using Jint.Native;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public class JavaScriptPlugin : IPlugin
    {
        public string PluginFile { get; private set; }

        public bool Enabled { get; private set; }

        public PluginProperties Properties { get; set; }

        private JintEngine _engine;

        private readonly Dictionary<string, JsFunction> _registeredEvents;
        private readonly Dictionary<string, JsFunction> _registeredCommands;
        private readonly Dictionary<string, JsFunction> _registeredConsoleCommands;

        public JavaScriptPlugin(string fileName)
        {
            PluginFile = fileName;

            _engine = new JintEngine();
            _engine.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            _engine.AllowClr = true;

            Properties = new PluginProperties(Path.GetFileName(PluginFile), PluginManager.PluginDirectoryPath);
            Properties.Load();

            _engine.SetParameter("properties", Properties);
            _engine.SetParameter("plugindir", PluginManager.PluginDirectoryPath);

            _engine.SetFunction("WriteToConsole", new Action<object>(Console.WriteLine));
            _engine.SetFunction("SubscribeToEvent", new Action<string, JsFunction>(SubscribeToEvent));
            _engine.SetFunction("UnsubscribeFromEvent", new Action<string>(UnsubscribeFromEvent));
            _engine.SetFunction("RegisterCommand", new Action<string, JsFunction>(RegisterCommand));
            _engine.SetFunction("UnregisterCommand", new Action<string>(UnregisterCommand));
            _engine.SetFunction("RegisterConsoleCommand", new Action<string, JsFunction>(RegisterConsoleCommand));
            _engine.SetFunction("UnregisterConsoleCommand", new Action<string>(UnregisterConsoleCommand));
            _engine.SetFunction("SendPacket", new Action<SharpStarClient, IPacket>(SendPacket));
            _engine.SetFunction("SendClientPacketToAll", new Action<IPacket>(SendClientPacketToAll));
            _engine.SetFunction("SendServerPacketToAll", new Action<IPacket>(SendServerPacketToAll));
            _engine.SetFunction("GetPlayerClients", new Func<IClient[]>(GetPlayerClients));
            _engine.SetFunction("GetServerClients", new Func<IClient[]>(GetServerClients));

            _engine.SetFunction("ToArray", new Func<JsArray, object[]>(JsArrayToArray));

            _registeredEvents = new Dictionary<string, JsFunction>();
            _registeredCommands = new Dictionary<string, JsFunction>();
            _registeredConsoleCommands = new Dictionary<string, JsFunction>();
        }

        public bool PassChatCommand(string command, IClient client, string[] args)
        {
            if (!Enabled)
                return false;

            var cmd = _registeredCommands.SingleOrDefault(p => p.Key.Equals(command, StringComparison.OrdinalIgnoreCase));

            if (cmd.Value != null)
            {
                try
                {
                    _engine.CallFunction(cmd.Value, new object[] {client, command, args});

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }

            return false;
        }

        public bool PassConsoleCommand(string command, string[] args)
        {
            if (!Enabled)
                return false;

            var cmd = _registeredConsoleCommands.SingleOrDefault(p => p.Key.Equals(command, StringComparison.OrdinalIgnoreCase));

            if (cmd.Value != null)
            {
                try
                {
                    _engine.CallFunction(cmd.Value, new object[] { command, args });

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }

            return false;
        }

        public void OnLoad()
        {
            try
            {
                _engine.Run(File.ReadAllText(PluginFile));

                Enabled = true;

                _engine.Run("onLoad();");
            }
            catch (Exception)
            {
            }
        }

        public void OnUnload()
        {
            try
            {
                _engine.Run("onUnload();");
            }
            catch (Exception)
            {
            }

            _registeredEvents.Clear();

            Enabled = false;
        }

        public IClient[] GetPlayerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => (IClient) p.PlayerClient).ToArray();
        }

        public IClient[] GetServerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => (IClient) p.ServerClient).ToArray();
        }

        public async void SendPacket(SharpStarClient client, IPacket packet)
        {
            if (!Enabled)
                return;

            client.PacketQueue.Enqueue(packet);
            await client.FlushPackets();
        }

        public void SendClientPacketToAll(IPacket packet)
        {
            if (!Enabled)
                return;

            foreach (SharpStarServerClient client in SharpStarMain.Instance.Server.Clients)
            {
                SendPacket(client.ServerClient, packet);
            }
        }

        public void SendServerPacketToAll(IPacket packet)
        {
            if (!Enabled)
                return;

            foreach (SharpStarServerClient client in SharpStarMain.Instance.Server.Clients)
            {
                SendPacket(client.PlayerClient, packet);
            }
        }

        public void CallEvent(string evtName, params object[] args)
        {
            if (!Enabled)
                return;

            if (_registeredEvents.ContainsKey(evtName))
            {
                try
                {
                    _engine.CallFunction(_registeredEvents[evtName], args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }
        }

        public void SubscribeToEvent(string eventName, JsFunction func)
        {
            _registeredEvents.Add(eventName, func);
        }

        public void UnsubscribeFromEvent(string eventName)
        {
            _registeredEvents.Remove(eventName);
        }

        public void RegisterCommand(string command, JsFunction func)
        {
            _registeredCommands.Add(command, func);
        }

        public void UnregisterCommand(string command)
        {
            _registeredCommands.Remove(command);
        }

        public void RegisterConsoleCommand(string command, JsFunction func)
        {
            _registeredConsoleCommands.Add(command, func);
        }

        public void UnregisterConsoleCommand(string command)
        {
            _registeredConsoleCommands.Remove(command);
        }

        public static object[] JsArrayToArray(JsArray array)
        {
            var objArr = new object[array.Length];

            int ctr = 0;
            foreach (JsInstance inst in array.GetValues())
            {
                objArr[ctr] = inst.ToObject();

                ctr++;
            }

            return objArr;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            _engine = null;
        }

        ~JavaScriptPlugin()
        {
            Dispose(false);
        }
    }
}