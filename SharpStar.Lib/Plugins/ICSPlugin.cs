using Mono.Addins;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    [TypeExtensionPoint]
    public interface ICSPlugin
    {

        string Name { get; }

        void OnLoad();

        void OnUnload();

        void OnPluginLoaded(ICSPlugin plugin);

        void OnPluginUnloaded(ICSPlugin plugin);

        void OnEventOccurred(string evtName, IPacket packet, StarboundClient client, params object[] args);

        bool OnChatCommandReceived(StarboundClient client, string command, string[] args);

        bool OnConsoleCommand(string command, string[] args);

    }
}
