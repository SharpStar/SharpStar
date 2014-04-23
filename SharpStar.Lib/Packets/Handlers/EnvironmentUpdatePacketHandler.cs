using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class EnvironmentUpdatePacketHandler : PacketHandler<EnvironmentUpdatePacket>
    {
        public override void Handle(EnvironmentUpdatePacket packet, StarboundClient client)
        {


            var coords = WorldCoordinate.GetGlobalCoords(packet.Sky);

            if (coords != null)
            {
                client.Server.Player.Coordinates = coords;
            }


            SharpStarMain.Instance.PluginManager.CallEvent("environmentUpdate", packet, client);

        }

        public override void HandleAfter(EnvironmentUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEnvironmentUpdate", packet, client);
        }
    }
}
