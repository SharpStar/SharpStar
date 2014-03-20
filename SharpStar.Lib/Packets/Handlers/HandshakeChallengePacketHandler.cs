using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class HandshakeChallengePacketHandler : PacketHandler<HandshakeChallengePacket>
    {
        public override void Handle(HandshakeChallengePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("handshakeChallenge", packet, client);
        }

        public override void HandleAfter(HandshakeChallengePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterHandshakeChallenge", packet, client);
        }
    }
}