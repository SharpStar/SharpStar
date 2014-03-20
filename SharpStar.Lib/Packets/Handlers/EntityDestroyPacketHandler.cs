using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class EntityDestroyPacketHandler : PacketHandler<EntityDestroyPacket>
    {
        public override void Handle(EntityDestroyPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("entityDestroy", packet, client);
        }

        public override void HandleAfter(EntityDestroyPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityDestroy", packet, client);
        }
    }
}