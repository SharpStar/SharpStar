using System;
using SharpStar.Lib.Database;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class ClientConnectPacketHandler : PacketHandler<ClientConnectPacket>
    {
        public override void Handle(ClientConnectPacket packet, StarboundClient client)
        {

            if (packet.IsReceive)
            {

                client.Server.Player = new StarboundPlayer(packet.PlayerName, BitConverter.ToString(packet.UUID, 0).Replace("-", String.Empty).ToLower());

                if (!string.IsNullOrEmpty(packet.Account))
                {

                    client.Server.Player.AttemptedLogin = true;

                    SharpStarUser user = SharpStarMain.Instance.Database.GetUser(packet.Account);

                    if (user == null)
                    {

                        client.Server.PlayerClient.SendPacket(new HandshakeChallengePacket { Salt = "" });

                        return;

                    }

                    if (user.GroupId.HasValue)
                    {

                        var group = SharpStarMain.Instance.Database.GetGroup(user.GroupId.Value);

                        if (group != null)
                        {
                            client.Server.Player.UserGroupId = group.Id;
                        }

                    }

                    packet.Account = "";

                    client.Server.Player.UserAccount = user;

                    client.Server.PlayerClient.SendPacket(new HandshakeChallengePacket { Salt = user.Salt });

                }

            }

            SharpStarMain.Instance.PluginManager.CallEvent("clientConnected", packet, client);
        }

        public override void HandleAfter(ClientConnectPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterClientConnected", packet, client);
        }
    }
}