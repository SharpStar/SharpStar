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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public class PyPlugin : IPlugin
    {

        private readonly Dictionary<string, string> _registeredEvents;

        private readonly Dictionary<string, string> _registeredCommands;

        private readonly Dictionary<string, string> _registeredConsoleCommands;

        public string PluginFile { get; private set; }

        public string MainClassName { get; private set; }

        public bool Enabled { get; private set; }

        private readonly ScriptEngine _engine;
        private readonly ScriptScope _scope;

        private ScriptSource _source;
        private CompiledCode _compiledCode;

        private PyPlugin _mainPythonClass;

        public PyPlugin()
        {
            _registeredEvents = new Dictionary<string, string>();
            _registeredCommands = new Dictionary<string, string>();
        }

        public PyPlugin(string fileName)
            : this()
        {

            PluginFile = fileName;
            MainClassName = Path.GetFileNameWithoutExtension(fileName);

            _engine = Python.CreateEngine();
            _scope = _engine.CreateScope();

            string pyLoc = SharpStarMain.Instance.Config.ConfigFile.PythonLibLocation;

            string libPath = Path.Combine(SharpStarMain.AssemblyPath, "pylib");

            if (!Directory.Exists(libPath))
            {
                Directory.CreateDirectory(libPath);
            }

            var paths = _engine.GetSearchPaths();
            paths.Add(libPath);

            if (!string.IsNullOrEmpty(pyLoc) && Directory.Exists(pyLoc))
            {
                paths.Add(pyLoc);
            }

            _engine.SetSearchPaths(paths);

            _engine.Runtime.IO.SetOutput(Console.OpenStandardOutput(), Encoding.ASCII);
            _engine.Runtime.IO.SetOutput(Console.OpenStandardError(), Encoding.ASCII);

            _engine.Runtime.LoadAssembly(typeof(PyPlugin).Assembly);

        }

        public void CallEvent(string evtName, params object[] args)
        {

            var cmd = _mainPythonClass._registeredEvents.SingleOrDefault(p => p.Key.Equals(evtName, StringComparison.OrdinalIgnoreCase));

            if (cmd.Value != null)
            {
                try
                {
                    _engine.Operations.InvokeMember(_mainPythonClass, cmd.Value, args.Take(2).ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }

        }

        public bool PassChatCommand(string command, IClient client, string[] args)
        {

            var cmd = _mainPythonClass._registeredCommands.SingleOrDefault(p => p.Key.Equals(command, StringComparison.OrdinalIgnoreCase));

            if (cmd.Value != null)
            {
                try
                {
                    return _engine.Operations.InvokeMember(_mainPythonClass, cmd.Value, client, args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }

            return false;

        }

        public bool PassConsoleCommand(string command, string[] args)
        {
            var cmd = _mainPythonClass._registeredConsoleCommands.SingleOrDefault(p => p.Key.Equals(command, StringComparison.OrdinalIgnoreCase));

            if (cmd.Value != null)
            {
                try
                {
                    return _engine.Operations.InvokeMember(_mainPythonClass, cmd.Value, args.Cast<object>().ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
                }
            }

            return false;
        }

        public void OnLoad()
        {

            _source = _engine.CreateScriptSourceFromFile(PluginFile);
            _compiledCode = _source.Compile();

            _compiledCode.Execute(_scope);

            _mainPythonClass = _engine.Operations.Invoke(_scope.GetVariable(MainClassName));

            try
            {
                _engine.Operations.InvokeMember(_mainPythonClass, "on_load");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Plugin {0} caused error: {1}", Path.GetFileName(PluginFile), ex.Message);
            }

        }

        public void subscribe_to_event(string evtName, string funcName)
        {
            _registeredEvents.Add(evtName, funcName);
        }

        public void unsubscribe_from_Event(string eventName)
        {
            _registeredEvents.Remove(eventName);
        }

        public void register_command(string command, string funcName)
        {
            _registeredCommands.Add(command, funcName);
        }

        public void unregister_command(string command)
        {
            _registeredCommands.Remove(command);
        }

        public void register_console_command(string command, string funcName)
        {
            _registeredConsoleCommands.Add(command, funcName);
        }

        public void unregister_console_command(string command)
        {
            _registeredConsoleCommands.Remove(command);
        }

        public IClient[] get_player_clients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => (IClient)p.PlayerClient).ToArray();
        }

        public IClient[] get_server_clients()
        {
            return SharpStarMain.Instance.Server.Clients.Select(p => (IClient)p.ServerClient).ToArray();
        }

        public void OnUnload()
        {
            try
            {
                _engine.Operations.InvokeMember(_mainPythonClass, "on_unload");
            }
            catch (Exception)
            {
            }
            finally
            {
                _engine.Execute(@"import sys
sys.exit(0)");
            }
        }

        public void Dispose()
        {
        }
    }
}
