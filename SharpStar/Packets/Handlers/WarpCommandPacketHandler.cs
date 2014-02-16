using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class WarpCommandPacketHandler : PacketHandler<WarpCommandPacket>
    {
        public override void Handle(WarpCommandPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("warpCommand", packet, client);
        }

        public override void HandleAfter(WarpCommandPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterWarpCommand", packet, client);
        }
    }
}
