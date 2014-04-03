using System;
using System.Collections.Generic;
using System.Linq;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public abstract class CSPlugin : ICSPlugin
    {

        public abstract string Name { get; }

        private readonly Dictionary<string, Action<IPacket, StarboundClient>> _registeredEvents;

        private readonly Dictionary<string, Action<StarboundClient, string[]>> _registeredCommands;

        protected CSPlugin()
        {
            _registeredEvents = new Dictionary<string, Action<IPacket, StarboundClient>>();
            _registeredCommands = new Dictionary<string, Action<StarboundClient, string[]>>();
        }

        public virtual void OnLoad()
        {
        }

        public virtual void OnUnload()
        {
        }

        public void RegisterEvent(string name, Action<IPacket, StarboundClient> toCall)
        {

            if (!_registeredEvents.ContainsKey(name))
            {
                _registeredEvents.Add(name, toCall);
            }

        }

        public void RegisterCommand(string name, Action<StarboundClient, string[]> toCall)
        {

            if (!_registeredCommands.ContainsKey(name))
            {
                _registeredCommands.Add(name, toCall);
            }

        }

        public void UnregisterCommand(string name)
        {

            if (_registeredCommands.ContainsKey(name))
            {
                _registeredCommands.Remove(name);
            }

        }

        public virtual void OnEventOccurred(string evtName, IPacket packet, StarboundClient client, params object[] args)
        {

            if (_registeredEvents.ContainsKey(evtName))
            {
                _registeredEvents[evtName](packet, client);
            }

        }

        public virtual bool OnChatCommandReceived(StarboundClient client, string command, string[] args)
        {

            var cmd = _registeredCommands.SingleOrDefault(p => p.Key.Equals(command, StringComparison.OrdinalIgnoreCase));

            if (cmd.Value != null)
            {

                cmd.Value(client, args);

                return true;

            }

            return false;

        }
    }
}
