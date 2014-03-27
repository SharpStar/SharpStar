using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Addins;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

[assembly: AddinRoot("SharpStar.Lib", "1.0")]

namespace SharpStar.Lib.Plugins
{
    public sealed class CSPluginManager
    {

        private readonly Dictionary<string, ICSPlugin> _csPlugins;

        public const string CSPluginDirectory = "addins";

        public CSPluginManager()
        {
            _csPlugins = new Dictionary<string, ICSPlugin>();
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

        public void PassChatCommand(StarboundClient client, string command, string[] args)
        {

            foreach (ICSPlugin plugin in _csPlugins.Values)
            {
                plugin.OnChatCommandReceived(client, command, args);
            }

        }

        public void UnloadPlugins()
        {

            if (AddinManager.IsInitialized)
            {

                foreach (ICSPlugin plugin in _csPlugins.Values)
                {
                    plugin.OnUnload();
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

                    _csPlugins.Add(Addin.GetFullId(null, args.ExtensionNode.Addin.Id, args.ExtensionNode.Addin.Version), plugin);

                    Console.WriteLine("Loaded CSharp Plugin \"{0}\"", plugin.Name);

                }
                else if (args.Change == ExtensionChange.Remove)
                {

                    plugin.OnUnload();

                    _csPlugins.Remove(Addin.GetFullId(null, args.ExtensionNode.Addin.Id, args.ExtensionNode.Addin.Version));

                    Console.WriteLine("Unloaded CSharp Plugin \"{0}\"", plugin.Name);

                }
            }

        }

        public void LoadPlugin(ICSPlugin plugin)
        {

            var plugins = _csPlugins.Where(p => p.Value.Equals(plugin)).ToList();

            if (plugins.Count > 1)
            {
                Console.WriteLine("Error loading plugin \"{0}\"", plugin.Name);
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
                Console.WriteLine("Error unloading plugin \"{0}\"", plugin.Name);
            }
            else if (plugins.Count > 0)
            {
                AddinManager.Registry.DisableAddin(plugins[0].Key);
            }

        }

    }
}
