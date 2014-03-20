using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class UniverseTimeUpdatePacketHandler : PacketHandler<UniverseTimeUpdatePacket>
    {
        public override void Handle(UniverseTimeUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("universeTimeUpdate", packet, client);
        }

        public override void HandleAfter(UniverseTimeUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterUniverseTimeUpdate", packet, client);
        }
    }
}