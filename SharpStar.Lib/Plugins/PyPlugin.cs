using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            if (!string.IsNullOrEmpty(pyLoc) && Directory.Exists(pyLoc))
            {

                var paths = _engine.GetSearchPaths();
                paths.Add(pyLoc);

                _engine.SetSearchPaths(paths);

            }

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
