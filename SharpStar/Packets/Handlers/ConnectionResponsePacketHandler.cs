using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class ConnectionResponsePacketHandler : ServerPacketHandler<ConnectionResponsePacket>
    {
        public override void Handle(ConnectionResponsePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("connectionResponse", packet, client);
        }

        public override void HandleAfter(ConnectionResponsePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterConnectionResponse", packet, client);
        }
    }
}
