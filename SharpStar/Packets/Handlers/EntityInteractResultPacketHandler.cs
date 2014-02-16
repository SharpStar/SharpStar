using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class EntityInteractResultPacketHandler : PacketHandler<EntityInteractResultPacket>
    {
        public override void Handle(EntityInteractResultPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("entityInteractResult", packet, client);
        }

        public override void HandleAfter(EntityInteractResultPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityInteractResult", packet, client);
        }
    }
}
