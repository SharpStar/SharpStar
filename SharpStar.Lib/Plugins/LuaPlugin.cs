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
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NLua;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public class LuaPlugin : IPlugin
    {
        private readonly Dictionary<string, LuaFunction> _registeredEvents;

        private readonly Dictionary<string, LuaFunction> _registeredCommands;

        private readonly Dictionary<string, LuaFunction> _registeredConsoleCommands;

        public string PluginFile { get; private set; }

        public bool Enabled { get; private set; }

        public PluginProperties Properties { get; set; }

        private Lua _lua;

        public LuaPlugin(string fileName)
        {
            PluginFile = fileName;

            _registeredEvents = new Dictionary<string, LuaFunction>();
            _registeredCommands = new Dictionary<string, LuaFunction>();
            _registeredConsoleCommands = new Dictionary<string, LuaFunction>();
        }

        private void RegisterTypes()
        {

            Assembly assm = GetType().Assembly;

            _lua.DoString(String.Format("luanet.load_assembly('{0}')", assm.GetName().Name));
            _lua.DoString(String.Format("luanet.load_assembly('{0}')", typeof (JObject).Assembly.GetName().Name));

            Type[] packetTypes =
                assm.GetTypes()
                    .Where(
                        p =>
                            String.Equals(p.Namespace, "SharpStar.Lib.Packets", StringComparison.Ordinal) &&
                            typeof (IPacket).IsAssignableFrom(p))
                    .ToArray();

            foreach (Type type in packetTypes)
            {
                _lua.DoString(String.Format("{0}=luanet.import_type('{1}')", type.Name, type.FullName));
            }

            Type[] otherTypes =
                assm.GetTypes().Where(p => String.Equals(p.Namespace, "SharpStar.Lib.DataTypes", StringComparison.Ordinal)
                                           || String.Equals(p.Namespace, "SharpStar.Lib.Misc")).ToArray();

            foreach (Type type in otherTypes)
            {
                _lua.DoString(String.Format("{0}=luanet.import_type('{1}')", type.Name, type.FullName));
            }

            _lua.DoString(String.Format("Direction=luanet.import_type('{0}')", typeof (Direction).FullName));

            _lua.DoString(String.Format("WarpType=luanet.import_type('{0}')", typeof (WarpType).FullName));

            _lua.DoString(String.Format("PluginProperties=luanet.import_type('{0}')", typeof (PluginProperties).FullName));

            _lua.DoString(String.Format("PluginProperty=luanet.import_type('{0}')", typeof (PluginProperty).FullName));
            _lua.DoString(String.Format("PluginPropertyArray=luanet.import_type('{0}')",
                typeof (PluginPropertyArray).FullName));
            _lua.DoString(String.Format("PluginPropertyObject=luanet.import_type('{0}')",
                typeof (PluginPropertyObject).FullName));
            _lua.DoString(String.Format("PluginPropertyValue=luanet.import_type('{0}')",
                typeof (PluginPropertyValue).FullName));

            _lua.DoString("ctype, enum = luanet.ctype, luanet.enum");
        }

        private void RegisterFunctions()
        {
            _lua.RegisterFunction("WriteToConsole", this, this.GetType().GetMethod("WriteToConsole"));
            _lua.RegisterFunction("SubscribeToEvent", this, this.GetType().GetMethod("SubscribeToEvent"));
            _lua.RegisterFunction("UnsubscribeFromEvent", this, this.GetType().GetMethod("UnsubscribeFromEvent"));
            _lua.RegisterFunction("RegisterCommand", this, this.GetType().GetMethod("RegisterCommand"));
            _lua.RegisterFunction("UnregisterCommand", this, this.GetType().GetMethod("UnregisterCommand"));
            _lua.RegisterFunction("RegisterConsoleCommand", this, this.GetType().GetMethod("RegisterConsoleCommand"));
            _lua.RegisterFunction("UnregisterConsoleCommand", this, this.GetType().GetMethod("UnregisterConsoleCommand"));
            _lua.RegisterFunction("SendPacket", this, this.GetType().GetMethod("SendPacket"));
            _lua.RegisterFunction("SendClientPacketToAll", this, this.GetType().GetMethod("SendClientPacketToAll"));
            _lua.RegisterFunction("SendServerPacketToAll", this, this.GetType().GetMethod("SendServerPacketToAll"));
            _lua.RegisterFunction("GetPlayerClients", this, this.GetType().GetMethod("GetPlayerClients"));
            _lua.RegisterFunction("GetServerClients", this, this.GetType().GetMethod("GetServerClients"));

            _lua.RegisterFunction("SplitString", this.GetType().GetMethod("SplitString"));
            _lua.RegisterFunction("JoinString", this.GetType().GetMethod("JoinString"));
            _lua.RegisterFunction("ToArray", this.GetType().GetMethod("LuaTableToArray"));
        }

        public IClient[] GetPlayerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => (IClient) p.PlayerClient).ToArray();
        }

        public IClient[] GetServerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => (IClient) p.ServerClient).ToArray();
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

        public void SendPacket(SharpStarClient client, IPacket packet)
        {
            if (!Enabled)
                return;

            if (client != null)
                client.SendPacket(packet);
        }

        public void SubscribeToEvent(string evtName, LuaFunction func)
        {
            _registeredEvents.Add(evtName, func);
        }

        public void UnsubscribeFromEvent(string eventName)
        {
            _registeredEvents.Remove(eventName);
        }

        public void RegisterCommand(string command, LuaFunction func)
        {
            _registeredCommands.Add(command, func);
        }

        public void UnregisterCommand(string command)
        {
            _registeredCommands.Remove(command);
        }

        public void RegisterConsoleCommand(string command, LuaFunction func)
        {
            _registeredConsoleCommands.Add(command, func);
        }

        public void UnregisterConsoleCommand(string command)
        {
            _registeredConsoleCommands.Remove(command);
        }

        public void CallEvent(string evtName, params object[] args)
        {
            if (!Enabled)
                return;

            if (_registeredEvents.ContainsKey(evtName))
            {
                try
                {
                    _registeredEvents[evtName].Call(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }
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
                    cmd.Value.Call(new object[] {client, command, args});

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
                    cmd.Value.Call(new object[] { command, args });

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
            Enabled = true;

            _registeredEvents.Clear();
            _registeredCommands.Clear();

            Properties = new PluginProperties(Path.GetFileName(PluginFile), PluginManager.PluginDirectoryPath);
            Properties.Load();

            _lua = new Lua();
            _lua.LoadCLRPackage();

            _lua["properties"] = Properties;
            _lua["plugindir"] = PluginManager.PluginDirectoryPath;

            _lua.DoString("package.path = package.path .. \";plugins/?.lua\"");

            RegisterTypes();
            RegisterFunctions();

            _lua.DoFile(PluginFile);

            LuaFunction onLoad = _lua.GetFunction("onLoad");

            if (onLoad != null)
            {
                try
                {
                    onLoad.Call();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }
        }

        public void OnUnload()
        {
            LuaFunction onUnload = _lua.GetFunction("onUnload");

            if (onUnload != null)
            {
                try
                {
                    onUnload.Call();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }

            _registeredEvents.Clear();

            Enabled = false;
        }

        public void WriteToConsole(object write)
        {
            Console.WriteLine(write);
        }

        public static object[] LuaTableToArray(LuaTable table)
        {
            var objArr = new object[table.Values.Count];

            int ctr = 0;
            foreach (var value in table.Values)
            {
                objArr[ctr] = value;

                ctr++;
            }

            return objArr;
        }

        public static string[] SplitString(string str, string pattern)
        {
            return Regex.Split(str, pattern);
        }

        public static string JoinString(string[] arr, string separator)
        {
            return string.Join(separator, arr);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Enabled = false;

            if (disposing)
            {
                _lua.Dispose();
            }

            _lua = null;
        }
    }
}