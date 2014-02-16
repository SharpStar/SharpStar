using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class UnknownPacketHandler : PacketHandler<UnknownPacket>
    {
        public override void Handle(UnknownPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("unknownPacket", packet, client);
        }

        public override void HandleAfter(UnknownPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterUnknownPacket", packet, client);
        }
    }
}
