using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class ClientDisconnectPacketHandler : PacketHandler<ClientDisconnectPacket>
    {
        public override void Handle(ClientDisconnectPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("clientDisconnected", packet, client);
        }

        public override void HandleAfter(ClientDisconnectPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterClientDisconnected", packet, client);
        }
    }
}
