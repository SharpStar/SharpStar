using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class WorldStopPacketHandler : PacketHandler<WorldStartPacket>
    {
        public override void Handle(WorldStartPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("worldStop", packet, client);
        }

        public override void HandleAfter(WorldStartPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterWorldStop", packet, client);
        }
    }
}
