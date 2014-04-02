using System;
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

                if (cmd.Equals("createacct", StringComparison.OrdinalIgnoreCase) && SharpStarMain.Instance.Config.ConfigFile.EnableAccounts)
                {

                    if (client.Server.Player.UserAccount != null)
                    {
                        client.SendChatMessage("Server", "You are already logged into an account!");
                    }
                    else if (args.Length == 2)
                    {

                        string username = args[0];
                        string password = args[1];

                        if (SharpStarMain.Instance.Database.AddUser(username, password))
                        {
                            client.SendChatMessage("Server", "Account created! Please reconnect with your login details.");
                        }
                        else
                        {
                            client.SendChatMessage("Server", "Account already exists!");
                        }

                    }

                    packet.Ignore = true;

                }

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