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
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public abstract class CSPlugin : ICSPlugin
    {

        public abstract string Name { get; }

        private readonly Dictionary<string, Action<string[]>> _registeredConsoleCommands;

        private readonly List<KnownPacket> _registeredEvents;

        private readonly List<KnownPacket> _registeredAfterEvents;

        public readonly Dictionary<object, List<CallableCommandEvent>> RegisteredCommandObjects;

        public readonly Dictionary<object, Dictionary<Tuple<string, string>, Action<string[]>>> RegisteredConsoleCommandObjects;

        public readonly Dictionary<object, List<CallablePacketEvent>> RegisteredPacketEventObjects;

        protected CSPlugin()
        {
            _registeredEvents = new List<KnownPacket>();
            _registeredAfterEvents = new List<KnownPacket>();
            _registeredConsoleCommands = new Dictionary<string, Action<string[]>>();

            RegisteredCommandObjects = new Dictionary<object, List<CallableCommandEvent>>();
            RegisteredPacketEventObjects = new Dictionary<object, List<CallablePacketEvent>>();
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
            throw new NotImplementedException();
            //if (!_registeredEvents.ContainsKey(name))
            //{
            //    _registeredEvents.Add(name, toCall);
            //}
        }

        public void RegisterCommandObject(object obj)
        {

            if (!RegisteredCommandObjects.ContainsKey(obj))
            {
                var list = new List<CallableCommandEvent>();

                MethodInfo[] methods = obj.GetType().GetMethods();

                foreach (var mi in methods)
                {

                    var attribs = mi.GetCustomAttributes(typeof(CommandAttribute), false).ToList();
                    var permAttribs = mi.GetCustomAttributes(typeof(CommandPermissionAttribute), false).ToList();

                    if (attribs.Count == 1)
                    {
                        CommandAttribute attrib = (CommandAttribute)attribs[0];

                        CallableCommandEvent ce;

                        if (mi.ReturnType == typeof(Task))
                        {
                            ce = new CallableCommandEvent(Delegate.CreateDelegate(typeof(Func<SharpStarClient, string[], Task>), obj, mi));
                            ce.IsAsync = true;
                        }
                        else
                        {
                            ce = new CallableCommandEvent(Delegate.CreateDelegate(typeof(Action<SharpStarClient, string[]>), obj, mi));
                        }

                        ce.CommandName = attrib.CommandName;
                        ce.CommandDescription = attrib.CommandDescription;

                        if (permAttribs.Count > 0)
                        {
                            CommandPermissionAttribute cpa = (CommandPermissionAttribute)permAttribs[0];

                            ce.CommandPermission = cpa.Permission;
                        }
                        else
                        {
                            ce.CommandDescription = String.Empty;
                        }

                        list.Add(ce);
                    }
                }

                RegisteredCommandObjects.Add(obj, list);
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
            if (!RegisteredPacketEventObjects.ContainsKey(obj))
            {
                var list = new List<CallablePacketEvent>();

                MethodInfo[] methods = obj.GetType().GetMethods();

                foreach (MethodInfo mi in methods)
                {
                    var attribs = mi.GetCustomAttributes(typeof(PacketEventAttribute), true).ToList();

                    if (attribs.Count == 1)
                    {
                        PacketEventAttribute attrib = (PacketEventAttribute)attribs[0];

                        bool isAsync = mi.ReturnType == typeof(Task);

                        Delegate toCall;
                        if (isAsync)
                        {
                            toCall = Delegate.CreateDelegate(typeof(Func<IPacket, SharpStarClient, Task>), obj, mi);
                        }
                        else
                        {
                            toCall = Delegate.CreateDelegate(typeof(Action<IPacket, SharpStarClient>), obj, mi);
                        }

                        bool isAfter = attrib is AfterPacketEventAttribute;

                        foreach (KnownPacket kp in attrib.PacketTypes)
                        {
                            CallablePacketEvent cevt = new CallablePacketEvent(toCall);
                            cevt.IsAfter = isAfter;
                            cevt.PacketType = kp;
                            cevt.IsAsync = isAsync;
                            cevt.ToCall = toCall;

                            if (!isAfter)
                            {
                                if (!_registeredEvents.Contains(kp))
                                    _registeredEvents.Add(kp);
                            }
                            else
                            {
                                if (!_registeredAfterEvents.Contains(kp))
                                    _registeredAfterEvents.Add(kp);
                            }

                            list.Add(cevt);
                        }
                    }
                }

                RegisteredPacketEventObjects.Add(obj, list);
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

        public async Task OnEventOccurred(IPacket packet, SharpStarClient client, bool isAfter = false)
        {
            KnownPacket kp = (KnownPacket)packet.PacketId;

            if (!isAfter && !_registeredEvents.Contains(kp))
                return;

            if (isAfter && !_registeredAfterEvents.Contains(kp))
                return;

            try
            {

                foreach (List<CallablePacketEvent> evts in RegisteredPacketEventObjects.Values)
                {
                    foreach (CallablePacketEvent evt in evts)
                    {
                        if (evt.PacketType == kp && evt.IsAfter == isAfter)
                        {
                            if (evt.IsAsync)
                            {
                                var toCall = evt.ToCall as Func<IPacket, SharpStarClient, Task>;

                                if (toCall != null)
                                {
                                    await toCall(packet, client);
                                }
                            }
                            else
                            {
                                var toCall = evt.ToCall as Action<IPacket, SharpStarClient>;

                                if (toCall != null)
                                {
                                    toCall(packet, client);
                                }
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                SharpStarLogger.DefaultLogger.Error("Plugin {0} has casued an error with packet event {1}!", Name, ((KnownPacket)packet.PacketId).ToString());

                if (ex.InnerException != null)
                {
                    SharpStarLogger.DefaultLogger.Error(ex.InnerException.ToString());
                }

                ex.LogError();
            }
        }

        public virtual async Task<bool> OnChatCommandReceived(SharpStarClient client, string command, string[] args)
        {
            bool contained = false;

            foreach (List<CallableCommandEvent> list in RegisteredCommandObjects.Values)
            {
                foreach (CallableCommandEvent cce in list)
                {
                    if (cce.CommandName.Equals(command, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(cce.CommandPermission))
                        {
                            if (cce.IsAsync)
                            {
                                var toCall = cce.ToCall as Func<SharpStarClient, string[], Task>;

                                if (toCall != null)
                                {
                                    await toCall(client, args);
                                }
                            }
                            else
                            {
                                var toCall = cce.ToCall as Action<SharpStarClient, string[]>;

                                if (toCall != null)
                                {
                                    toCall(client, args);
                                }
                            }
                        }
                        else if ((!string.IsNullOrEmpty(cce.CommandPermission) && client.Server.Player == null) || (client.Server.Player != null &&
                            !client.Server.Player.HasPermission(cce.CommandPermission)))
                        {
                            OnCommandPermissionDenied(client, cce.CommandName);
                        }
                        else
                        {
                            if (cce.IsAsync)
                            {
                                var toCall = cce.ToCall as Func<SharpStarClient, string[], Task>;

                                if (toCall != null)
                                {
                                    await toCall(client, args);
                                }
                            }
                            else
                            {
                                var toCall = cce.ToCall as Action<SharpStarClient, string[]>;

                                if (toCall != null)
                                {
                                    toCall(client, args);
                                }
                            }
                        }

                        contained = true;
                    }
                }
            }

            return contained;
        }

        public virtual bool OnConsoleCommand(string command, string[] args)
        {
            var cmd = _registeredConsoleCommands.SingleOrDefault(p => p.Key.Equals(command, StringComparison.OrdinalIgnoreCase));

            bool contained = false;

            foreach (var kvp in RegisteredConsoleCommandObjects)
            {
                var val = kvp.Value.SingleOrDefault(p => p.Key.Item1.Equals(command, StringComparison.OrdinalIgnoreCase));

                if (val.Value != null)
                {
                    val.Value(args);

                    contained = true;
                }
            }

            if (cmd.Value != null)
            {
                cmd.Value(args);

                contained = true;
            }

            return contained;
        }

        public virtual void OnCommandPermissionDenied(SharpStarClient client, string command)
        {
            client.SendChatMessage("Server", String.Format("Permission denied for command {0}!", command));
        }
    }
}
