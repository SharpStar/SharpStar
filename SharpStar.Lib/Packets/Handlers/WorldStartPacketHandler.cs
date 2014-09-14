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
using System.Threading.Tasks;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class WorldStartPacketHandler : PacketHandler<WorldStartPacket>
    {
        public override Task Handle(WorldStartPacket packet, SharpStarClient client)
        {
            Variant planet = packet.Planet;
            VariantDict planetDict = (VariantDict) planet.Value;

            if (planetDict.ContainsKey("celestialParameters"))
            {
                var celestParamsDict = (VariantDict) planetDict["celestialParameters"].Value;

                if (celestParamsDict != null)
                {
                    var coordinateDict = (VariantDict) celestParamsDict["coordinate"].Value;

                    if (coordinateDict != null)
                    {
                        var loc = (Variant[]) coordinateDict["location"].Value;

                        var pCoords = new PlanetCoordinate
                        {
                            Sector = (string) coordinateDict["sector"].Value,
                            X = (ulong) loc[0].Value,
                            Y = (ulong) loc[1].Value,
                            Z = (ulong) loc[2].Value,
                            Planet = (ulong) coordinateDict["planet"].Value,
                            Satellite = (ulong) coordinateDict["satellite"].Value
                        };

                        client.Server.Player.OnShip = false;
                    }
                }
                else
                {
                    client.Server.Player.OnShip = true;
                }
            }

            var coords = WorldCoordinate.GetGlobalCoords(packet.Sky);

            if (coords != null)
            {
                client.Server.Player.Coordinates = coords;
            }

            client.Server.Player.ClientId = packet.ClientId;
            client.Server.Player.SpawnX = packet.SpawnX;
            client.Server.Player.SpawnY = packet.SpawnY;

            SharpStarMain.Instance.PluginManager.CallEvent("worldStart", packet, client);

            return base.Handle(packet, client);
        }

        public override Task HandleAfter(WorldStartPacket packet, SharpStarClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterWorldStart", packet, client);

            return base.HandleAfter(packet, client);
        }
    }
}