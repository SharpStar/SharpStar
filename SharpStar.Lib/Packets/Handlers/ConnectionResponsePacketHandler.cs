using System;
using System.IO;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class ConnectionResponsePacketHandler : PacketHandler<ConnectionResponsePacket>
    {
        public override void Handle(ConnectionResponsePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("connectionResponse", packet, client);
        }

        public override void HandleAfter(ConnectionResponsePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterConnectionResponse", packet, client);
        }
    }
}