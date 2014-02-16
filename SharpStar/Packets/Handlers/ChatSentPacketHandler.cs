using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public class ChatSentPacketHandler : PacketHandler<ChatSentPacket>
    {
        public override void Handle(ChatSentPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("chatSent", packet, client);
        }

        public override void HandleAfter(ChatSentPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterChatSent", packet, client);
        }
    }
}
