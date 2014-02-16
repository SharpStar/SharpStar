using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
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
