using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class DisconnectResponsePacketHandler : PacketHandler<DisconnectResponsePacket>
    {
        public override void Handle(DisconnectResponsePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("disconnectResponse", packet, client);
        }

        public override void HandleAfter(DisconnectResponsePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterDisconnectResponse", packet, client);
        }
    }
}
