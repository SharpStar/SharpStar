using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NLua;
using SharpStar.Packets;
using SharpStar.Server;

namespace SharpStar.Plugins
{
    public class LuaPlugin : IPlugin
    {

        private readonly Dictionary<string, LuaFunction> _registeredEvents;

        public string PluginFile { get; private set; }

        public bool Enabled { get; private set; }

        public PluginProperties Properties { get; set; }

        private Lua _lua;

        public LuaPlugin(string fileName)
        {

            PluginFile = fileName;

            _registeredEvents = new Dictionary<string, LuaFunction>();

        }

        private void RegisterTypes()
        {

            var assm = Assembly.GetExecutingAssembly();

            _lua.DoString(String.Format("luanet.load_assembly('{0}')", assm.GetName().Name));
            _lua.DoString(String.Format("luanet.load_assembly('{0}')", typeof(JObject).Assembly.GetName().Name));

            Type[] packetTypes = assm.GetTypes().Where(p => String.Equals(p.Namespace, "SharpStar.Packets", StringComparison.Ordinal) && typeof(IPacket).IsAssignableFrom(p)).ToArray();

            foreach (Type type in packetTypes)
            {
                _lua.DoString(String.Format("{0}=luanet.import_type('{1}')", type.Name, type.FullName));
            }

            Type[] otherTypes = assm.GetTypes().Where(p => String.Equals(p.Namespace, "SharpStar.DataTypes", StringComparison.Ordinal)
                || String.Equals(p.Namespace, "SharpStar.Misc")).ToArray();

            foreach (Type type in otherTypes)
            {
                _lua.DoString(String.Format("{0}=luanet.import_type('{1}')", type.Name, type.FullName));
            }

            _lua.DoString(String.Format("Direction=luanet.import_type('{0}')", typeof(Direction).FullName));

            _lua.DoString(String.Format("WarpType=luanet.import_type('{0}')", typeof(WarpType).FullName));

            _lua.DoString(String.Format("PluginProperties=luanet.import_type('{0}')", typeof(PluginProperties).FullName));

            _lua.DoString(String.Format("PluginProperty=luanet.import_type('{0}')", typeof(PluginProperty).FullName));
            _lua.DoString(String.Format("PluginPropertyArray=luanet.import_type('{0}')", typeof(PluginPropertyArray).FullName));
            _lua.DoString(String.Format("PluginPropertyObject=luanet.import_type('{0}')", typeof(PluginPropertyObject).FullName));
            _lua.DoString(String.Format("PluginPropertyValue=luanet.import_type('{0}')", typeof(PluginPropertyValue).FullName));

            _lua.DoString("ctype, enum = luanet.ctype, luanet.enum");

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

            _lua.RegisterFunction("SplitString", this.GetType().GetMethod("SplitString"));
            _lua.RegisterFunction("ToArray", this.GetType().GetMethod("LuaTableToArray"));

        }

        public StarboundClient[] GetPlayerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => p.PlayerClient).ToArray();
        }

        public StarboundClient[] GetServerClients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => p.ServerClient).ToArray();
        }

        public void SendClientPacketToAll(IPacket packet)
        {

            if (!Enabled)
                return;

            foreach (StarboundServerClient client in SharpStarMain.Instance.Server.Clients)
            {
                SendPacket(client.ServerClient, packet);
            }

        }

        public void SendServerPacketToAll(IPacket packet)
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

        public void OnLoad()
        {

            Enabled = true;

            _registeredEvents.Clear();

            Properties = new PluginProperties(Path.GetFileName(PluginFile), PluginManager.PluginDirectoryPath);
            Properties.Load();

            _lua = new Lua();
            _lua.LoadCLRPackage();

            _lua["properties"] = Properties;
            _lua["plugindir"] = PluginManager.PluginDirectoryPath;

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
