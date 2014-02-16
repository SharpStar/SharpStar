using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class EntityDestroyPacketHandler : PacketHandler<EntityDestroyPacket>
    {
        public override void Handle(EntityDestroyPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("entityDestroy", packet, client);
        }

        public override void HandleAfter(EntityDestroyPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityDestroy", packet, client);
        }
    }
}
