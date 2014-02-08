using System;

namespace SharpStar.Plugins
{
    public interface IPlugin : IDisposable
    {

        string PluginFile { get; }

        bool Enabled { get; }

        void CallEvent(string evtName, params object[] args);

        void OnLoad();

        void OnUnload();

    }
}
