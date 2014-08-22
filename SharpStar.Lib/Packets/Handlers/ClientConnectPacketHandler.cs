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
using System.Linq;
using SharpStar.Lib.Database;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class ClientConnectPacketHandler : PacketHandler<ClientConnectPacket>
    {
        public override void Handle(ClientConnectPacket packet, SharpStarClient client)
        {

            if (packet.IsReceive)
            {

                string uuid = BitConverter.ToString(packet.UUID, 0).Replace("-", String.Empty).ToLower();

                var clients = SharpStarMain.Instance.Server.Clients.Where(p => p.Player != null && p.Player.UUID == uuid);
                clients.ToList().ForEach(p =>
                {
                    SharpStarLogger.DefaultLogger.Info("Duplicate UUID ({0}) detected. Killing old client!", uuid);
                    p.PlayerClient.ForceDisconnect();
                    p.ServerClient.ForceDisconnect();
                });

                client.Server.Player = new StarboundPlayer(packet.PlayerName, uuid)
                {
                    Claim = packet.Claim,
                    Species = packet.Species,
                    ShipWorld = packet.Shipworld,
                    AssetDigest = packet.AssetDigest,
                    OnOwnShip = true
                };

                if (!string.IsNullOrEmpty(packet.Account.Trim()))
                {

                    client.Server.Player.AttemptedLogin = true;

                    SharpStarUser user = SharpStarMain.Instance.Database.GetUser(packet.Account.Trim());

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

        public override void HandleAfter(ClientConnectPacket packet, SharpStarClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterClientConnected", packet, client);
        }
    }
}