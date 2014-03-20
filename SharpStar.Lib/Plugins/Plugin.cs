using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public abstract class Plugin : IPlugin
    {
        public abstract string PluginFile { get; protected set; }
        public abstract bool Enabled { get; protected set; }

        public virtual bool PassChatCommand(string command, IClient client, string[] args)
        {
            return false;
        }

        public virtual void OnLoad()
        {
        }

        public virtual void OnUnload()
        {
        }

        public virtual void CallEvent(string evtName, params object[] args)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}