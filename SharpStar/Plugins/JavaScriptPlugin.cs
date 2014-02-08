using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using Jint;
using Jint.Native;
using SharpStar.Packets;
using SharpStar.Server;

namespace SharpStar.Plugins
{
    public class JavaScriptPlugin : IPlugin
    {

        public string PluginFile { get; private set; }
        public bool Enabled { get; private set; }

        public PluginProperties Properties { get; set; }

        private JintEngine _engine;

        private readonly Dictionary<string, JsFunction> _registeredEvents;

        public JavaScriptPlugin(string fileName)
        {

            PluginFile = fileName;

            _engine = new JintEngine();
            _engine.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            _engine.AllowClr = true;

            Properties = new PluginProperties(Path.GetFileName(PluginFile), PluginManager.PluginDirectoryPath);
            Properties.Load();

            _engine.SetParameter("properties", Properties);

            _engine.SetFunction("WriteToConsole", new Action<object>(Console.WriteLine));
            _engine.SetFunction("SubscribeToEvent", new Action<string, JsFunction>(SubscribeToEvent));
            _engine.SetFunction("SendPacket", new Action<StarboundClient, IPacket>(SendPacket));
            _engine.SetFunction("SendClientPacketToAll", new Action<ClientPacket>(SendClientPacketToAll));
            _engine.SetFunction("SendServerPacketToAll", new Action<ServerPacket>(SendServerPacketToAll));
            _engine.SetFunction("GetPlayerClients", new Func<StarboundClient[]>(GetPlayerClients));
            _engine.SetFunction("GetServerClients", new Func<StarboundClient[]>(GetServerClients));

            _registeredEvents = new Dictionary<string, JsFunction>();
        
        }

        public void OnLoad()
        {

            try
            {
                _engine.Run(File.ReadAllText(PluginFile));

                Enabled = true;
                
                _engine.Run("onLoad();");
            }
            catch
            {
            }

        }

        public void OnUnload()
        {

            try
            {
                _engine.Run("onUnload();");
            }
            catch
            {
            }

            _registeredEvents.Clear();

            Enabled = false;

        }

        public StarboundClient[] GetPlayerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => p.PlayerClient).ToArray();
        }

        public StarboundClient[] GetServerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => p.ServerClient).ToArray();
        }

        public void SendPacket(StarboundClient client, IPacket packet)
        {

            if (!Enabled)
                return;
            
            client.PacketQueue.Enqueue(packet);
            client.FlushPackets();
        
        }

        public void SendClientPacketToAll(ClientPacket packet)
        {

            if (!Enabled)
                return;

            foreach (StarboundServerClient client in SharpStarMain.Instance.Server.Clients)
            {
                SendPacket(client.ServerClient, packet);
            }
        
        }

        public void SendServerPacketToAll(ServerPacket packet)
        {

            if (!Enabled)
                return;

            foreach (StarboundServerClient client in SharpStarMain.Instance.Server.Clients)
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

    }
}
