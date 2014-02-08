using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using NLua;
using SharpStar.DataTypes;
using SharpStar.Packets;
using SharpStar.Server;

namespace SharpStar.Plugins
{
    public class LuaPlugin : IPlugin
    {

        private readonly Dictionary<string, string> _registeredEvents;

        public string PluginFile { get; private set; }

        public bool Enabled { get; private set; }

        public PluginProperties Properties { get; set; }

        private Lua _lua;

        public LuaPlugin(string fileName)
        {

            PluginFile = fileName;

            _registeredEvents = new Dictionary<string, string>();

        }

        private void RegisterTypes()
        {

            _lua.DoString("luanet.load_assembly('mscorlib')");
            _lua.DoString("luanet.load_assembly('SharpStar')");

            var assm = Assembly.GetExecutingAssembly();
            Type[] types = assm.GetTypes().Where(p => String.Equals(p.Namespace, "SharpStar.Packets", StringComparison.Ordinal) && typeof(IPacket).IsAssignableFrom(p)).ToArray();

            foreach (Type type in types)
            {
                _lua.DoString(String.Format("{0}=luanet.import_type('{1}')", type.Name, type.FullName));
            }

            _lua.DoString(String.Format("WorldCoordinate=luanet.import_type('{0}')", typeof(WorldCoordinate).FullName));
            _lua.DoString(String.Format("WarpType=luanet.import_type('{0}')", typeof(WarpType).FullName));
            _lua.DoString(String.Format("Variant=luanet.import_type('{0}')", typeof(Variant).FullName));

        }

        private void RegisterFunctions()
        {
            _lua.RegisterFunction("WriteToConsole", this, this.GetType().GetMethod("WriteToConsole"));
            _lua.RegisterFunction("SubscribeToEvent", this, this.GetType().GetMethod("SubscribeToEvent"));
            _lua.RegisterFunction("SendPacket", this, this.GetType().GetMethod("SendPacket"));
            _lua.RegisterFunction("SendClientPacketToAll", this, this.GetType().GetMethod("SendClientPacketToAll"));
            _lua.RegisterFunction("SendServerPacketToAll", this, this.GetType().GetMethod("SendServerPacketToAll"));
            _lua.RegisterFunction("GetPlayerClients", this, this.GetType().GetMethod("GetPlayerClients"));
            _lua.RegisterFunction("GetServerClients", this, this.GetType().GetMethod("GetServerClients"));
        }

        public StarboundClient[] GetPlayerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => p.PlayerClient).ToArray();
        }

        public StarboundClient[] GetServerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => p.ServerClient).ToArray();
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

        public void SendPacket(StarboundClient client, IPacket packet)
        {

            if (!Enabled)
                return;

            client.SendPacket(packet);

        }

        public void SubscribeToEvent(string evtName, string funcName)
        {
            _registeredEvents.Add(evtName, funcName);
        }

        public void UnsubscribeFromEvent(string eventName)
        {
            _registeredEvents.Remove(eventName);
        }

        public void CallEvent(string evtName, params object[] args)
        {

            if (!Enabled)
                return;

            if (_registeredEvents.ContainsKey(evtName))
            {
                try
                {
                    _lua.GetFunction(_registeredEvents[evtName]).Call(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }

        }

        public void OnLoad()
        {

            Enabled = true;

            _registeredEvents.Clear();

            Properties = new PluginProperties(Path.GetFileName(PluginFile), PluginManager.PluginDirectoryPath);
            Properties.Load();

            _lua = new Lua();
            _lua.LoadCLRPackage();

            _lua["properties"] = Properties;

            RegisterTypes();
            RegisterFunctions();

            _lua.DoFile(PluginFile);

            LuaFunction onLoad = _lua.GetFunction("onLoad");

            if (onLoad != null)
                onLoad.Call();

        }

        public void OnUnload()
        {

            LuaFunction onUnload = _lua.GetFunction("onUnload");

            if (onUnload != null)
                onUnload.Call();

            _registeredEvents.Clear();

            Enabled = false;

        }

        public void WriteToConsole(string write)
        {
            Console.WriteLine(write);
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
