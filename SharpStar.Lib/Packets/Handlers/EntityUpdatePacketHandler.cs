using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class EntityUpdatePacketHandler : PacketHandler<EntityUpdatePacket>
    {
        public override void Handle(EntityUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("entityUpdate", packet, client);
        }

        public override void HandleAfter(EntityUpdatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityUpdate", packet, client);
        }
    }
}