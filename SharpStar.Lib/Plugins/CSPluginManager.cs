using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Addins;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

[assembly: AddinRoot("SharpStar.Lib", "1.0")]

namespace SharpStar.Lib.Plugins
{
    public sealed class CSPluginManager
    {

        private static readonly SharpStarLogger Logger = SharpStarLogger.DefaultLogger;

        private readonly object _commandLocker = new object();

        public readonly List<Tuple<string, string, string, bool>> Commands;

        public readonly Dictionary<string, string> ConsoleCommands;

        private readonly Dictionary<string, ICSPlugin> _csPlugins;

        public List<ICSPlugin> Plugins
        {
            get
            {
                return _csPlugins.Values.ToList();
            }
        }

        public const string CSPluginDirectory = "addins";

        public CSPluginManager()
        {
            _csPlugins = new Dictionary<string, ICSPlugin>();
            Commands = new List<Tuple<string, string, string, bool>>();
            ConsoleCommands = new Dictionary<string, string>();
        }

        public void LoadPlugins()
        {

            if (!Directory.Exists(CSPluginDirectory))
                Directory.CreateDirectory(CSPluginDirectory);

            if (!AddinManager.IsInitialized)
            {
                AddinManager.Initialize(".", "./addins");
                AddinManager.AddExtensionNodeHandler(typeof(ICSPlugin), OnExtensionChanged);
            }

            foreach (Addin addin in AddinManager.Registry.GetAddins())
            {
                AddinManager.Registry.EnableAddin(addin.Id);
            }

            AddinManager.Registry.Update(null);

        }

        public void CallEvent(string evtName, IPacket packet, StarboundClient client, params object[] args)
        {
            foreach (ICSPlugin csPlugin in _csPlugins.Values)
            {
                csPlugin.OnEventOccurred(evtName, packet, client, args);
            }
        }

        public bool PassChatCommand(StarboundClient client, string command, string[] args)
        {
            bool result = false;

            foreach (ICSPlugin plugin in _csPlugins.Values)
            {
                if (plugin.OnChatCommandReceived(client, command, args))
                    result = true;
            }

            return result;
        }

        public bool PassConsoleCommand(string command, string[] args)
        {
            bool result = false;

            foreach (ICSPlugin plugin in _csPlugins.Values)
            {
                if (plugin.OnConsoleCommand(command, args))
                    result = true;
            }

            return result;
        }

        public void UnloadPlugins()
        {

            if (AddinManager.IsInitialized)
            {

                foreach (ICSPlugin plugin in _csPlugins.Values)
                {
                    plugin.OnUnload();

                    foreach (ICSPlugin otherPlugin in _csPlugins.Values.Except(new[] { plugin }))
                    {
                        otherPlugin.OnPluginUnloaded(plugin);
                    }
                }

                foreach (Addin addin in AddinManager.Registry.GetAddins())
                {
                    AddinManager.Registry.DisableAddin(addin.Id);
                }

            }

        }

        private void OnExtensionChanged(object sender, ExtensionNodeEventArgs args)
        {

            var plugin = args.ExtensionObject as ICSPlugin;

            if (plugin != null)
            {
                if (args.Change == ExtensionChange.Add)
                {

                    plugin.OnLoad();

                    foreach (ICSPlugin p in _csPlugins.Values)
                    {
                        p.OnPluginLoaded(plugin);
                    }

                    _csPlugins.Add(Addin.GetFullId(null, args.ExtensionNode.Addin.Id, args.ExtensionNode.Addin.Version), plugin);

                    Logger.Info("Loaded CSharp Plugin \"{0}\"", plugin.Name);

                }
                else if (args.Change == ExtensionChange.Remove)
                {

                    plugin.OnUnload();

                    foreach (ICSPlugin p in _csPlugins.Values)
                        p.OnPluginUnloaded(plugin);

                    _csPlugins.Remove(Addin.GetFullId(null, args.ExtensionNode.Addin.Id, args.ExtensionNode.Addin.Version));

                    Logger.Info("Unloaded CSharp Plugin \"{0}\"", plugin.Name);

                }

                RefreshCommands();

            }

        }

        public void LoadPlugin(ICSPlugin plugin)
        {

            var plugins = _csPlugins.Where(p => p.Value.Equals(plugin)).ToList();

            if (plugins.Count > 1)
            {
                Logger.Error("Error loading plugin \"{0}\"", plugin.Name);
            }
            else if (plugins.Count > 0)
            {
                AddinManager.Registry.EnableAddin(plugins[0].Key);
            }

        }

        public void UnloadPlugin(ICSPlugin plugin)
        {

            var plugins = _csPlugins.Where(p => p.Value.Equals(plugin)).ToList();

            if (plugins.Count > 1)
            {
                Logger.Error("Error unloading plugin \"{0}\"", plugin.Name);
            }
            else if (plugins.Count > 0)
            {
                AddinManager.Registry.DisableAddin(plugins[0].Key);
            }

        }

        private void RefreshCommands()
        {
            lock (_commandLocker)
            {
                Commands.Clear();
                ConsoleCommands.Clear();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (Assembly assm in assemblies.Where(p => p.GetTypes().Any(x => typeof(ICSPlugin).IsAssignableFrom(x))))
                {

                    Type[] types = assm.GetTypes();

                    foreach (Type type in types)
                    {

                        foreach (MethodInfo mi in type.GetMethods())
                        {

                            var cmdAttribs = mi.GetCustomAttributes(typeof(CommandAttribute), false).ToList();
                            var consoleCmdAttribs = mi.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).ToList();

                            if (consoleCmdAttribs.Count == 1)
                            {

                                ConsoleCommandAttribute cCmdAttr = (ConsoleCommandAttribute)cmdAttribs[0];

                                ConsoleCommands.Add(cCmdAttr.CommandName, cCmdAttr.CommandDescription);

                            }
                            else if (cmdAttribs.Count == 1)
                            {

                                var permAttribs = mi.GetCustomAttributes(typeof(CommandPermissionAttribute), false).ToList();

                                CommandAttribute cmdAttrib = (CommandAttribute)cmdAttribs[0];

                                if (permAttribs.Count == 1)
                                {
                                    CommandPermissionAttribute permAttrib = (CommandPermissionAttribute)permAttribs[0];

                                    Commands.Add(Tuple.Create(cmdAttrib.CommandName, cmdAttrib.CommandDescription, permAttrib.Permission, permAttrib.Admin));
                                }
                                else
                                {
                                    Commands.Add(Tuple.Create(cmdAttrib.CommandName, cmdAttrib.CommandDescription, String.Empty, false));
                                }

                            }

                        }

                    }

                }
            }
        }

    }
}
