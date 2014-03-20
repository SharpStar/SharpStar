using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    internal class HeartbeatPacketHandler : PacketHandler<HeartbeatPacket>
    {
        public override void Handle(HeartbeatPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("heartbeat", packet, client);
        }

        public override void HandleAfter(HeartbeatPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterHeartbeat", packet, client);
        }
    }
}