using System.Linq;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class ChatSentPacketHandler : PacketHandler<ChatSentPacket>
    {
        public override void Handle(ChatSentPacket packet, StarboundClient client)
        {

            SharpStarMain.Instance.PluginManager.CallEvent("chatSent", packet, client);

            string message = packet.Message;

            if (message.StartsWith("/"))
            {
                string[] ex = message.Substring(1).Split(' ');

                string cmd = ex[0];

                string[] args = ex.Skip(1).ToArray();

                if (SharpStarMain.Instance.PluginManager.PassChatCommand(client, cmd, args))
                    packet.Ignore = true;
            }
        }

        public override void HandleAfter(ChatSentPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterChatSent", packet, client);
        }
    }
}