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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class EnvironmentUpdatePacketHandler : PacketHandler<EnvironmentUpdatePacket>
    {
        public override Task Handle(EnvironmentUpdatePacket packet, SharpStarClient client)
        {
            var coords = WorldCoordinate.GetGlobalCoords(packet.Sky);

            if (coords != null)
            {
                client.Server.Player.Coordinates = coords;
            }

            SharpStarMain.Instance.PluginManager.CallEvent("environmentUpdate", packet, client);

            return base.Handle(packet, client);
        }

        public override Task HandleAfter(EnvironmentUpdatePacket packet, SharpStarClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEnvironmentUpdate", packet, client);

            return base.HandleAfter(packet, client);
        }
    }
}
