using System;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public interface IPlugin : IDisposable
    {

        string PluginFile { get; }

        bool Enabled { get; }

        void CallEvent(string evtName, params object[] args);

        bool PassChatCommand(string command, IClient client, string[] args);

        bool PassConsoleCommand(string command, string[] args);

        void OnLoad();

        void OnUnload();

    }
}
