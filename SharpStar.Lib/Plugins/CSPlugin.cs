﻿// SharpStar
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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public abstract class CSPlugin : ICSPlugin
    {

        public abstract string Name { get; }

        private readonly Dictionary<string, Action<IPacket, SharpStarClient>> _registeredEvents;

        private readonly Dictionary<string, Action<SharpStarClient, string[]>> _registeredCommands;

        private readonly Dictionary<string, Action<string[]>> _registeredConsoleCommands;


        public readonly Dictionary<object, Dictionary<Tuple<string, string>, Action<SharpStarClient, string[]>>> RegisteredCommandObjects;

        public readonly Dictionary<object, Dictionary<Tuple<string, string>, Action<string[]>>> RegisteredConsoleCommandObjects;

        public readonly Dictionary<object, Dictionary<string, Action<IPacket, SharpStarClient>>> RegisteredEventObjects;

        protected CSPlugin()
        {
            _registeredEvents = new Dictionary<string, Action<IPacket, SharpStarClient>>();
            _registeredCommands = new Dictionary<string, Action<SharpStarClient, string[]>>();
            _registeredConsoleCommands = new Dictionary<string, Action<string[]>>();

            RegisteredCommandObjects = new Dictionary<object, Dictionary<Tuple<string, string>, Action<SharpStarClient, string[]>>>();
            RegisteredEventObjects = new Dictionary<object, Dictionary<string, Action<IPacket, SharpStarClient>>>();
            RegisteredConsoleCommandObjects = new Dictionary<object, Dictionary<Tuple<string, string>, Action<string[]>>>();
        }

        public virtual void OnLoad()
        {
        }

        public virtual void OnUnload()
        {
        }

        public virtual void OnPluginLoaded(ICSPlugin plugin)
        {
        }

        public virtual void OnPluginUnloaded(ICSPlugin plugin)
        {
        }

        public void RegisterEvent(string name, Action<IPacket, SharpStarClient> toCall)
        {
            if (!_registeredEvents.ContainsKey(name))
            {
                _registeredEvents.Add(name, toCall);
            }
        }

        public void RegisterCommandObject(object obj)
        {

            if (!RegisteredCommandObjects.ContainsKey(obj))
            {
                var dict = new Dictionary<Tuple<string, string>, Action<SharpStarClient, string[]>>();

                MethodInfo[] methods = obj.GetType().GetMethods();

                foreach (var mi in methods)
                {

                    var attribs = mi.GetCustomAttributes(typeof(CommandAttribute), false).ToList();

                    if (attribs.Count == 1)
                    {
                        CommandAttribute attrib = (CommandAttribute)attribs[0];

                        dict.Add(Tuple.Create(attrib.CommandName, attrib.CommandDescription), (Action<SharpStarClient, string[]>)Delegate.CreateDelegate(typeof(Action<SharpStarClient, string[]>), obj, mi));
                    }

                }

                RegisteredCommandObjects.Add(obj, dict);
            }

        }

        public void UnregisterCommandObject(object obj)
        {
            RegisteredCommandObjects.Remove(obj);
        }

        public void RegisterConsoleCommandObject(object obj)
        {
            if (!RegisteredConsoleCommandObjects.ContainsKey(obj))
            {
                var dict = new Dictionary<Tuple<string, string>, Action<string[]>>();

                MethodInfo[] methods = obj.GetType().GetMethods();

                foreach (var mi in methods)
                {
                    var attribs = mi.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).ToList();

                    if (attribs.Count == 1)
                    {
                        ConsoleCommandAttribute attrib = (ConsoleCommandAttribute)attribs[0];

                        dict.Add(Tuple.Create(attrib.CommandName, attrib.CommandDescription), (Action<string[]>)Delegate.CreateDelegate(typeof(Action<string[]>), obj, mi));
                    }
                }

                RegisteredConsoleCommandObjects.Add(obj, dict);
            }
        }

        public void UnregisterConsoleCommandObject(object obj)
        {
            RegisteredConsoleCommandObjects.Remove(obj);
        }

        public void RegisterEventObject(object obj)
        {
            if (!RegisteredEventObjects.ContainsKey(obj))
            {
                var dict = new Dictionary<string, Action<IPacket, SharpStarClient>>();

                MethodInfo[] methods = obj.GetType().GetMethods();

                foreach (var mi in methods)
                {
                    var attribs = mi.GetCustomAttributes(typeof(EventAttribute), false).ToList();

                    if (attribs.Count == 1)
                    {

                        EventAttribute attrib = (EventAttribute)attribs[0];

                        var act = (Action<IPacket, SharpStarClient>)Delegate.CreateDelegate(typeof(Action<IPacket, SharpStarClient>), obj, mi);

                        foreach (string evt in attrib.EventNames)
                            dict.Add(evt, act);

                    }
                }

                RegisteredEventObjects.Add(obj, dict);
            }
        }

        public void RegisterCommand(string name, Action<SharpStarClient, string[]> toCall)
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

        public void RegisterConsoleCommand(string name, Action<string[]> toCall)
        {
            if (_registeredConsoleCommands.ContainsKey(name))
            {
                _registeredConsoleCommands.Add(name, toCall);
            }
        }

        public void UnregisterConsoleCommand(string name)
        {
            if (_registeredConsoleCommands.ContainsKey(name))
            {
                _registeredConsoleCommands.Remove(name);
            }
        }

        public virtual void OnEventOccurred(string evtName, IPacket packet, SharpStarClient client, params object[] args)
        {
            if (_registeredEvents.ContainsKey(evtName))
            {
                _registeredEvents[evtName](packet, client);
            }

            Parallel.ForEach(RegisteredEventObjects.Where(p => p.Value.ContainsKey(evtName)), kvp =>
            {
                if (kvp.Value.ContainsKey(evtName))
                {
                    kvp.Value[evtName](packet, client);
                }
            });
        }

        public virtual bool OnChatCommandReceived(SharpStarClient client, string command, string[] args)
        {
            var cmd = _registeredCommands.SingleOrDefault(p => p.Key.Equals(command, StringComparison.OrdinalIgnoreCase));

            bool contained = false;

            Parallel.ForEach(RegisteredCommandObjects, kvp =>
            {
                var val = kvp.Value.SingleOrDefault(p => p.Key.Item1.Equals(command, StringComparison.OrdinalIgnoreCase));

                if (val.Value != null)
                {
                    val.Value(client, args);

                    contained = true;
                }
            });

            if (cmd.Value != null)
            {
                cmd.Value(client, args);

                contained = true;
            }

            return contained;
        }

        public virtual bool OnConsoleCommand(string command, string[] args)
        {
            var cmd = _registeredConsoleCommands.SingleOrDefault(p => p.Key.Equals(command, StringComparison.OrdinalIgnoreCase));

            bool contained = false;

            Parallel.ForEach(RegisteredConsoleCommandObjects, kvp =>
            {
                var val = kvp.Value.SingleOrDefault(p => p.Key.Item1.Equals(command, StringComparison.OrdinalIgnoreCase));

                if (val.Value != null)
                {
                    val.Value(args);

                    contained = true;
                }
            });

            if (cmd.Value != null)
            {
                cmd.Value(args);

                contained = true;
            }

            return contained;
        }
    }
}
