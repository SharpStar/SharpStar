using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class DamageNotificationPacketHandler : ClientPacketHandler<DamageNotificationPacket>
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
