using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class CloseContainerPacketHandler : PacketHandler<CloseContainerPacket>
    {
        public override void Handle(CloseContainerPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("closeContainer", packet, client);
        }

        public override void HandleAfter(CloseContainerPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterCloseContainer", packet, client);
        }
    }
}