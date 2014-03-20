using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class UpdateWorldPropertiesPacketHandler : PacketHandler<UpdateWorldPropertiesPacket>
    {
        public override void Handle(UpdateWorldPropertiesPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("updateWorldProperties", packet, client);
        }

        public override void HandleAfter(UpdateWorldPropertiesPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterUpdateWorldProperties", packet, client);
        }
    }
}