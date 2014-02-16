using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;
using SharpStar.DataTypes;

namespace SharpStar.Packets.Handlers
{
    public class WorldStartPacketHandler : ServerPacketHandler<WorldStartPacket>
    {
        public override void Handle(WorldStartPacket packet, StarboundClient client)
        {

            Variant planet = packet.Planet;
            VariantDict planetDict = (VariantDict)planet.Value;

            if (planetDict.ContainsKey("config") && planetDict["config"].Value is VariantDict)
            {

                var configDict = (VariantDict)planetDict["config"].Value;
                var coordinateDict = (VariantDict)configDict["coordinate"].Value;

                if (coordinateDict != null)
                {

                    var parentSystem = (VariantDict)coordinateDict["parentSystem"].Value;

                    var loc = (Variant[])parentSystem["location"].Value;

                    var coords = new PlanetCoordinate();
                    coords.Sector = (string)parentSystem["sector"].Value;
                    coords.X = (ulong)loc[0].Value;
                    coords.Y = (ulong)loc[1].Value;
                    coords.Z = (ulong)loc[2].Value;
                    coords.Planet = (ulong)coordinateDict["planetaryOrbitNumber"].Value;
                    coords.Satellite = (ulong)coordinateDict["satelliteOrbitNumber"].Value;

                    client.Server.Player.OnShip = false;
                    client.Server.Player.Coordinates = coords;

                }
                else
                {
                    client.Server.Player.OnShip = true;
                }

            }

            SharpStarMain.Instance.PluginManager.CallEvent("worldStart", packet, client);

        }

        public override void HandleAfter(WorldStartPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterWorldStart", packet, client);
        }
    }
}
