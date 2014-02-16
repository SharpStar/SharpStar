using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class GiveItemPacketHandler : PacketHandler<GiveItemPacket>
    {
        public override void Handle(GiveItemPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("giveItem", packet, client);
        }

        public override void HandleAfter(GiveItemPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterGiveItem", packet, client);
        }
    }
}
