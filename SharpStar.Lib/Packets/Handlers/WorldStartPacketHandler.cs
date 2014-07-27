using System;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class WorldStartPacketHandler : PacketHandler<WorldStartPacket>
    {
        public override void Handle(WorldStartPacket packet, StarboundClient client)
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
        }

        public override void HandleAfter(WorldStartPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterWorldStart", packet, client);
        }
    }
}