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

        void OnEventOccurred(string evtName, IPacket packet, StarboundClient client, params object[] args);

        bool OnChatCommandReceived(StarboundClient client, string command, string[] args);

    }
}
