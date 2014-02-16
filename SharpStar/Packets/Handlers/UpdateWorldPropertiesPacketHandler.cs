using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class UpdateWorldPropertiesPacketHandler : PacketHandler<UpdateWorldPropertiesPacket>
    {
        public override void Handle(UpdateWorldPropertiesPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("updateWorldProperties", packet, client);
        }

        public override void HandleAfter(UpdateWorldPropertiesPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterUpdateWorldProperties", packet, client);
        }
    }
}
