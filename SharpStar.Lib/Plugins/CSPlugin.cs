using System;
using System.Collections.Generic;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public abstract class CSPlugin : ICSPlugin
    {

        public abstract string Name { get; }

        private readonly Dictionary<string, Action<IPacket, StarboundClient>> _registeredEvents;

        protected CSPlugin()
        {
            _registeredEvents = new Dictionary<string, Action<IPacket, StarboundClient>>();
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

        public virtual void OnEventOccurred(string evtName, IPacket packet, StarboundClient client, params object[] args)
        {

            if (_registeredEvents.ContainsKey(evtName))
            {
                _registeredEvents[evtName](packet, client);
            }

        }

        public virtual void OnChatCommandReceived(StarboundClient client, string command, string[] args)
        {
        }
    }
}
