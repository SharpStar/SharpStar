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
using System.IO;
using System.Linq;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class ConnectionResponsePacketHandler : PacketHandler<ConnectionResponsePacket>
    {
        public override void Handle(ConnectionResponsePacket packet, SharpStarClient client)
        {
            if (packet.IsReceive)
            {
                if (client.Server.Player.UserAccount == null && SharpStarMain.Instance.Config.ConfigFile.RequireAccountLogin)
                {
                    packet.Success = false;
                    packet.RejectionReason = SharpStarMain.Instance.Config.ConfigFile.RequireAccountLoginError;
                }
                else if (client.Server.Player.UserAccount != null)
                {
                    SharpStarMain.Instance.Database.UpdateUserLastLogin(client.Server.Player.UserAccount.Username, DateTime.Now);
                }

                if (packet.Success && client.Server.Player != null && !string.IsNullOrEmpty(client.Server.Player.Name))
                    SharpStarLogger.DefaultLogger.Info("Player {0} has successfully joined!", client.Server.Player.Name);
            }

            SharpStarMain.Instance.PluginManager.CallEvent("connectionResponse", packet, client);
        }

        public override void HandleAfter(ConnectionResponsePacket packet, SharpStarClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterConnectionResponse", packet, client);
        }
    }
}