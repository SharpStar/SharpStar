using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class OpenContainerPacketHandler : PacketHandler<OpenContainerPacket>
    {
        public override void Handle(OpenContainerPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("openContainer", packet, client);
        }

        public override void HandleAfter(OpenContainerPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterOpenContainer", packet, client);
        }
    }
}