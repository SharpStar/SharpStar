using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class ChatReceivedPacketHandler : PacketHandler<ChatReceivedPacket>
    {
        public override void Handle(ChatReceivedPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("chatReceived", packet, client);
        }

        public override void HandleAfter(ChatReceivedPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterChatReceived", packet, client);
        }
    }
}