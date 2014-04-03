using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public sealed class PluginManager : IPluginManager
    {
        private readonly object _pluginLocker = new object();

        private readonly CSPluginManager _csPluginManager;

        public const string PluginDirectory = "plugins";

        public static string PluginDirectoryPath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PluginDirectory);

        private readonly List<IPlugin> _plugins;

        public PluginManager()
        {
            _plugins = new List<IPlugin>();
            _csPluginManager = new CSPluginManager();
        }

        public void LoadPlugins()
        {
            Console.WriteLine("Loading plugins...");

            if (!Directory.Exists(PluginDirectoryPath))
                Directory.CreateDirectory(PluginDirectoryPath);

            foreach (FileInfo fInfo in new DirectoryInfo(PluginDirectoryPath).GetFiles())
            {
                LoadPlugin(fInfo.FullName);
            }

            _csPluginManager.LoadPlugins();

            Console.WriteLine("Plugins Loaded!");
        }


        public void LoadPlugin(string file)
        {
            if (_plugins.Any(p => p.PluginFile == file))
                throw new Exception("This plugin has already been loaded!");

            FileInfo fInfo = new FileInfo(file);

            if (!fInfo.Exists)
                throw new FileNotFoundException("Could not find plugin file!");

            if (fInfo.Extension == ".js")
            {
                IPlugin plugin = new JavaScriptPlugin(fInfo.FullName);

                plugin.OnLoad();

                lock (_pluginLocker)
                    _plugins.Add(plugin);

                Console.WriteLine("Loaded JavaScript plugin {0}", fInfo.Name);
            }
            else if (fInfo.Extension == ".lua")
            {
                IPlugin plugin = new LuaPlugin(fInfo.FullName);

                plugin.OnLoad();

                lock (_pluginLocker)
                    _plugins.Add(plugin);

                Console.WriteLine("Loaded Lua plugin {0}", fInfo.Name);
            }
            else if (fInfo.Extension == ".py")
            {

                IPlugin plugin = new PyPlugin(fInfo.FullName);

                plugin.OnLoad();

                lock (_pluginLocker)
                    _plugins.Add(plugin);

                Console.WriteLine("Loaded Python plugin {0}", fInfo.Name);

            }

        }

        public void CallEvent(string evtName, IPacket packet, StarboundClient client, params object[] args)
        {
            lock (_pluginLocker)
            {
                _csPluginManager.CallEvent(evtName, packet, client, args);

                foreach (IPlugin plugin in _plugins)
                {
                    plugin.CallEvent(evtName, packet, client, args);
                }
            }
        }

        public bool PassChatCommand(StarboundClient client, string command, string[] args)
        {

            bool anyRegistered;

            lock (_pluginLocker)
            {

                anyRegistered = _csPluginManager.PassChatCommand(client, command, args);

                foreach (IPlugin plugin in _plugins)
                {
                    if (plugin.PassChatCommand(command, client, args))
                        anyRegistered = true;
                }

            }

            return anyRegistered;
        }

        public void UnloadPlugin(string file)
        {
            IPlugin plugin = _plugins.SingleOrDefault(p => new FileInfo(file).FullName == p.PluginFile);

            if (plugin != null)
            {
                try
                {
                    plugin.OnUnload();
                }
                catch (Exception)
                {
                }

                lock (_pluginLocker)
                {
                    _plugins.Remove(plugin);
                }

                Console.WriteLine("Unloaded plugin {0}", Path.GetFileName(file));
            }
            else
            {
                Console.WriteLine("Could not unload plugin {0}!", new FileInfo(file).Name);
            }
        }

        public void UnloadPlugins()
        {
            _csPluginManager.UnloadPlugins();

            foreach (IPlugin plugin in _plugins)
            {
                try
                {
                    plugin.OnUnload();
                    plugin.Dispose();
                }
                catch (Exception)
                {
                }
            }

            lock (_pluginLocker)
                _plugins.Clear();

            Console.WriteLine("All plugins have been unloaded!");
        }

        public void Reload()
        {
            UnloadPlugins();
            LoadPlugins();
        }
    }
}