using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class RequestDropPacketHandler : ClientPacketHandler<RequestDropPacket>
    {
        public override void Handle(RequestDropPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("requestDrop", packet, client);
        }

        public override void HandleAfter(RequestDropPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterRequestDrop", packet, client);
        }
    }
}
