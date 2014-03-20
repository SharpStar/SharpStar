using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class WorldStopPacketHandler : PacketHandler<WorldStartPacket>
    {
        public override void Handle(WorldStartPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("worldStop", packet, client);
        }

        public override void HandleAfter(WorldStartPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterWorldStop", packet, client);
        }
    }
}