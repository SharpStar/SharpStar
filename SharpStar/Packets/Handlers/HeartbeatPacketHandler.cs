using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    class HeartbeatPacketHandler : ServerPacketHandler<HeartbeatPacket>
    {
        public override void Handle(HeartbeatPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("heartbeat", packet, client);
        }

        public override void HandleAfter(HeartbeatPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterHeartbeat", packet, client);
        }
    }
}
