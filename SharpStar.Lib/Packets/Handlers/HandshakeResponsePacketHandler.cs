using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class HandshakeResponsePacketHandler : PacketHandler<HandshakeResponsePacket>
    {
        public override void Handle(HandshakeResponsePacket packet, StarboundClient client)
        {

            if (packet.IsReceive)
            {

                packet.Ignore = true;

                if (client.Server.Player.UserAccount != null && client.Server.Player.UserAccount.Hash != packet.PasswordHash)
                    client.Server.Player.UserAccount = null;

            }

            //SharpStarMain.Instance.PluginManager.CallEvent("handshakeResponse", packet, client);
        }

        public override void HandleAfter(HandshakeResponsePacket packet, StarboundClient client)
        {
            //SharpStarMain.Instance.PluginManager.CallEvent("afterHandshakeResponse", packet, client);
        }
    }
}
