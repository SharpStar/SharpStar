using System;
using SharpStar.Lib.Security;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class HandshakeChallengePacketHandler : PacketHandler<HandshakeChallengePacket>
    {
        public override void Handle(HandshakeChallengePacket packet, StarboundClient client)
        {

            if (packet.IsReceive)
            {
                packet.Ignore = true;

                if (client.Server.Player.UserAccount != null || !client.Server.Player.AttemptedLogin)
                {
                    client.Server.ServerClient.SendPacket(new HandshakeResponsePacket { PasswordHash = SharpStarSecurity.GenerateHash("", "", packet.Salt, 5000) });
                }
                else if (client.Server.Player.AttemptedLogin)
                {
                    client.Server.ServerClient.SendPacket(new HandshakeResponsePacket { PasswordHash = packet.Salt });
                }
            }

            //SharpStarMain.Instance.PluginManager.CallEvent("handshakeChallenge", packet, client);

        }

        public override void HandleAfter(HandshakeChallengePacket packet, StarboundClient client)
        {
            //SharpStarMain.Instance.PluginManager.CallEvent("afterHandshakeChallenge", packet, client);
        }
    }
}