using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SharpStar.Plugins
{
    public class PluginManager : IPluginManager
    {

        private static readonly object _pluginLocker = new object();

        public const string PluginDirectory = "plugins";

        public static string PluginDirectoryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PluginDirectory);

        private List<IPlugin> _plugins;

        public PluginManager()
        {
            _plugins = new List<IPlugin>();
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

                _plugins.Add(plugin);

                Console.WriteLine("Loaded JavaScript plugin {0}", fInfo.Name);

            }
            else if (fInfo.Extension == ".lua")
            {

                IPlugin plugin = new LuaPlugin(fInfo.FullName);

                plugin.OnLoad();

                _plugins.Add(plugin);

                Console.WriteLine("Loaded Lua plugin {0}", fInfo.Name);

            }

        }

        public void CallEvent(string evtName, params object[] args)
        {
            lock (_pluginLocker)
            {
                foreach (IPlugin plugin in _plugins)
                {
                    plugin.CallEvent(evtName, args);
                }
            }
        }

        public void UnloadPlugin(string file)
        {

            IPlugin plugin = _plugins.SingleOrDefault(p => new FileInfo(file).FullName == p.PluginFile);

            if (plugin != null)
            {
                
                plugin.OnUnload();
               
                _plugins.Remove(plugin);

                Console.WriteLine("Unloaded plugin {0}", Path.GetFileName(file));

            }
            else
            {
                Console.WriteLine("Could not unload plugin {0}!", new FileInfo(file).Name);
            }

        }

        public void UnloadPlugins()
        {

            foreach (IPlugin plugin in _plugins)
            {
                plugin.OnUnload();
                plugin.Dispose();
            }

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
