using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class RequestDropPacketHandler : PacketHandler<RequestDropPacket>
    {
        public override void Handle(RequestDropPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("requestDrop", packet, client);
        }

        public override void HandleAfter(RequestDropPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterRequestDrop", packet, client);
        }
    }
}