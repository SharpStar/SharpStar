using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class DamageNotificationPacketHandler : PacketHandler<DamageNotificationPacket>
    {
        public override void Handle(DamageNotificationPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("damageNotification", packet, client);
        }

        public override void HandleAfter(DamageNotificationPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterDamageNotification", packet, client);
        }
    }
}