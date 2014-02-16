using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class EntityCreatePacketHandler : ServerPacketHandler<EntityCreatePacket>
    {
        public override void Handle(EntityCreatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("entityCreate", packet, client);
        }

        public override void HandleAfter(EntityCreatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityCreate", packet, client);
        }
    }
}
