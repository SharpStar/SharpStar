// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Security;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class HandshakeChallengePacketHandler : PacketHandler<HandshakeChallengePacket>
    {
        public override void Handle(HandshakeChallengePacket packet, SharpStarClient client)
        {

            if (packet.IsReceive)
            {
                packet.Ignore = true;

                if (client.Server.Player.UserAccount != null || !client.Server.Player.AttemptedLogin)
                {
                    client.Server.ServerClient.SendPacket(new HandshakeResponsePacket { PasswordHash = SharpStarSecurity.GenerateHash("", "", packet.Salt, StarboundConstants.Rounds) });
                }
                else if (client.Server.Player.AttemptedLogin)
                {
                    client.Server.ServerClient.SendPacket(new HandshakeResponsePacket { PasswordHash = packet.Salt });
                }
            }

            //SharpStarMain.Instance.PluginManager.CallEvent("handshakeChallenge", packet, client);

        }

        public override void HandleAfter(HandshakeChallengePacket packet, SharpStarClient client)
        {
            //SharpStarMain.Instance.PluginManager.CallEvent("afterHandshakeChallenge", packet, client);
        }
    }
}