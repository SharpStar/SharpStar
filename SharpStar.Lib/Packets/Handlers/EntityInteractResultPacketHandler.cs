using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class EntityInteractResultPacketHandler : PacketHandler<EntityInteractResultPacket>
    {
        public override void Handle(EntityInteractResultPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("entityInteractResult", packet, client);
        }

        public override void HandleAfter(EntityInteractResultPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityInteractResult", packet, client);
        }
    }
}