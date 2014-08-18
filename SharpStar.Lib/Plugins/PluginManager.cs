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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public sealed class PluginManager : IPluginManager
    {

        private readonly object _pluginLocker = new object();

        private readonly CSPluginManager _csPluginManager;

        private static readonly SharpStarLogger Logger = SharpStarLogger.DefaultLogger;

        public CSPluginManager CSPluginManager
        {
            get
            {
                return _csPluginManager;
            }
        }

        public const string PluginDirectory = "plugins";

        public static string PluginDirectoryPath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PluginDirectory);

        private readonly List<IPlugin> _plugins;

        public List<IPlugin> Plugins
        {
            get
            {
                return _plugins.ToList();
            }
        }

        public PluginManager()
        {
            _plugins = new List<IPlugin>();
            _csPluginManager = new CSPluginManager();
        }

        public void LoadPlugins()
        {
            Logger.Info("Loading plugins...");

            if (!Directory.Exists(PluginDirectoryPath))
                Directory.CreateDirectory(PluginDirectoryPath);

            foreach (FileInfo fInfo in new DirectoryInfo(PluginDirectoryPath).GetFiles())
            {
                LoadPlugin(fInfo.FullName);
            }

            _csPluginManager.LoadPlugins();

            Logger.Info("Plugins Loaded!");
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

                Logger.Info("Loaded JavaScript plugin {0}", fInfo.Name);
            }
            else if (fInfo.Extension == ".lua")
            {
                IPlugin plugin = new LuaPlugin(fInfo.FullName);

                plugin.OnLoad();

                lock (_pluginLocker)
                    _plugins.Add(plugin);

                Logger.Info("Loaded Lua plugin {0}", fInfo.Name);
            }
            else if (fInfo.Extension == ".py")
            {

                IPlugin plugin = new PyPlugin(fInfo.FullName);

                plugin.OnLoad();

                lock (_pluginLocker)
                    _plugins.Add(plugin);

                Logger.Info("Loaded Python plugin {0}", fInfo.Name);

            }

        }

        public void CallEvent(string evtName, IPacket packet, SharpStarClient client, params object[] args)
        {
            _csPluginManager.CallEvent(evtName, packet, client, args);

            Parallel.ForEach(_plugins, plugin => plugin.CallEvent(evtName, packet, client, args));
        }

        public bool PassConsoleCommand(string command, string[] args)
        {
            bool anyRegistered;

            anyRegistered = _csPluginManager.PassConsoleCommand(command, args);

            Parallel.ForEach(_plugins, plugin =>
            {
                if (plugin.PassConsoleCommand(command, args))
                    anyRegistered = true;
            });

            return anyRegistered;
        }

        public bool PassChatCommand(SharpStarClient client, string command, string[] args)
        {

            bool anyRegistered;

            anyRegistered = _csPluginManager.PassChatCommand(client, command, args);

            Parallel.ForEach(_plugins, plugin =>
            {
                if (plugin.PassChatCommand(command, client, args))
                    anyRegistered = true;
            });

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

                _plugins.Remove(plugin);

                Logger.Info("Unloaded plugin {0}", Path.GetFileName(file));
            }
            else
            {
                Logger.Info("Could not unload plugin {0}!", new FileInfo(file).Name);
            }
        }

        public void UnloadPlugins()
        {
            _csPluginManager.UnloadPlugins();

            Parallel.ForEach(_plugins, plugin =>
            {
                try
                {
                    plugin.OnUnload();
                    plugin.Dispose();
                }
                catch (Exception)
                {
                }
            });

            lock (_pluginLocker)
                _plugins.Clear();

            Logger.Info("All plugins have been unloaded!");
        }

        public void Reload()
        {
            UnloadPlugins();
            LoadPlugins();
        }

    }
}