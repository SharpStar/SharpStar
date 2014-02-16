using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class EntityUpdatePacketHandler : PacketHandler<EntityUpdatePacket>
    {
        public override void Handle(EntityUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("entityUpdate", packet, client);
        }

        public override void HandleAfter(EntityUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityUpdate", packet, client);
        }
    }
}
