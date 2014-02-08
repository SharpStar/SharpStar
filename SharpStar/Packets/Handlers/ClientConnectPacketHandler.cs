using System;
using SharpStar.Entities;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class ClientConnectPacketHandler : ClientPacketHandler<ClientConnectPacket>
    {
        public override void Handle(ClientConnectPacket packet, StarboundClient client)
        {

            client.Server.Player = new StarboundPlayer(packet.PlayerName, new Guid(packet.UUID));

            SharpStarMain.Instance.PluginManager.CallEvent("clientConnected", packet, client);
        
        }

        public override void HandleAfter(ClientConnectPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterClientConnected", packet, client);
        }
    }
}
