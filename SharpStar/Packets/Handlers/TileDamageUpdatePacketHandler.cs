using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class TileDamageUpdatePacketHandler : PacketHandler<TileDamageUpdatePacket>
    {
        public override void Handle(TileDamageUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("tileDamageUpdate", packet, client);
        }

        public override void HandleAfter(TileDamageUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterTileDamageUpdate", packet, client);
        }
    }
}
