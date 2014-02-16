using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class OpenContainerPacketHandler : ClientPacketHandler<OpenContainerPacket>
    {
        public override void Handle(OpenContainerPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("openContainer", packet, client);
        }

        public override void HandleAfter(OpenContainerPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterOpenContainer", packet, client);
        }
    }
}
