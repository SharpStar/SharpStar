using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class UniverseTimeUpdatePacketHandler : ServerPacketHandler<UniverseTimeUpdatePacket>
    {
        public override void Handle(UniverseTimeUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("universeTimeUpdate", packet, client);
        }

        public override void HandleAfter(UniverseTimeUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterUniverseTimeUpdate", packet, client);
        }
    }
}
