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
using System.Threading.Tasks;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class EntityCreatePacketHandler : PacketHandler<EntityCreatePacket>
    {
        public override Task Handle(EntityCreatePacket packet, SharpStarClient client)
        {
            foreach (var ent in packet.Entities)
            {
                if (ent.EntityType == EntityType.Player && ent is PlayerEntity)
                {
                    PlayerEntity pent = (PlayerEntity)ent;

                    if (pent.UUID == client.Server.Player.UUID)
                    {
                        client.Server.Player.EntityId = ent.EntityId;
                    }
                }
            }

            SharpStarMain.Instance.PluginManager.CallEvent("entityCreate", packet, client);

            return base.Handle(packet, client);
        }

        public override Task HandleAfter(EntityCreatePacket packet, SharpStarClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityCreate", packet, client);

            return base.HandleAfter(packet, client);
        }
    }
}