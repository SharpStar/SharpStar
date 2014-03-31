using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class ProtocolVersionPacketHandler : PacketHandler<ProtocolVersionPacket>
    {
        public override void Handle(ProtocolVersionPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("protocolVersion", packet, client);
        }

        public override void HandleAfter(ProtocolVersionPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterProtocolVersion", packet, client);
        }
    }
}
