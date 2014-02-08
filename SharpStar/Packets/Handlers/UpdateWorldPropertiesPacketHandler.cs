using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class UpdateWorldPropertiesPacketHandler : ServerPacketHandler<UpdateWorldPropertiesPacket>
    {
        public override void Handle(UpdateWorldPropertiesPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("updateWorldProperties");
        }

        public override void HandleAfter(UpdateWorldPropertiesPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterUpdateWorldProperties");
        }
    }
}
