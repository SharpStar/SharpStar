using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class ClientContextUpdatePacketHandler : PacketHandler<ClientContextUpdatePacket>
    {
        public override void Handle(ClientContextUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("clientContextUpdate", packet, client);
        }

        public override void HandleAfter(ClientContextUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterClientContextUpdate", packet, client);
        }
    }
}