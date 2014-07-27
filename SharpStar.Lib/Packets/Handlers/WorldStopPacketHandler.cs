using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class WorldStopPacketHandler : PacketHandler<WorldStopPacket>
    {
        public override void Handle(WorldStopPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("worldStop", packet, client);
        }

        public override void HandleAfter(WorldStopPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterWorldStop", packet, client);
        }
    }
}