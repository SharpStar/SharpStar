using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class ClientContextUpdatePacketHandler : ServerPacketHandler<ClientContextUpdatePacket>
    {
        public override void Handle(ClientContextUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("clientContextUpdate", packet, client);
        }

        public override void HandleAfter(ClientContextUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterClientContextUpdate", packet, client);
        }
    }
}
