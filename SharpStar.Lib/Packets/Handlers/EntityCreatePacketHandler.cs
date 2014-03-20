using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class EntityCreatePacketHandler : PacketHandler<EntityCreatePacket>
    {
        public override void Handle(EntityCreatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("entityCreate", packet, client);
        }

        public override void HandleAfter(EntityCreatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityCreate", packet, client);
        }
    }
}