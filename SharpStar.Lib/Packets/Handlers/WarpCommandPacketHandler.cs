using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class WarpCommandPacketHandler : PacketHandler<WarpCommandPacket>
    {
        public override void Handle(WarpCommandPacket packet, StarboundClient client)
        {

            if (packet.WarpType == WarpType.WarpHome)
            {
                client.Server.Player.PlayerShip = null;
                client.Server.Player.OnOwnShip = true;
            }
            else if (packet.WarpType == WarpType.WarpOtherShip)
            {
                client.Server.Player.PlayerShip = packet.Player;
                client.Server.Player.OnOwnShip = false;
            }
            else
            {
                client.Server.Player.OnOwnShip = false;
            }

            SharpStarMain.Instance.PluginManager.CallEvent("warpCommand", packet, client);
        
        }

        public override void HandleAfter(WarpCommandPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterWarpCommand", packet, client);
        }
    }
}