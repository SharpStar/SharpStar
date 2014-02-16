using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class CloseContainerPacketHandler : ClientPacketHandler<CloseContainerPacket>
    {
        public override void Handle(CloseContainerPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("closeContainer", packet, client);
        }

        public override void HandleAfter(CloseContainerPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterCloseContainer", packet, client);
        }
    }
}
