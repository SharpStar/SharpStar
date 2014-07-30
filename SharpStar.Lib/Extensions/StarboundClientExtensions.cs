using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Extensions
{
    public static class StarboundClientExtensions
    {

        public static bool IsAdmin(this StarboundClient client)
        {
            return client.Server.Player.UserAccount != null && client.Server.Player.UserAccount.IsAdmin;
        }

        public static bool CanUserAccess(this StarboundClient client, string command, bool sendMsg = false)
        {
            if (client.IsAdmin())
                return true;

            var cmds = SharpStarMain.Instance.PluginManager.CSPluginManager.Commands;

            var cmd = cmds.SingleOrDefault(p => p.Item1 == command);

            if (cmd == null || cmd.Item4)
                return false;

            if (string.IsNullOrEmpty(cmd.Item3))
                return true;

            bool hasPerm = client.Server.Player.HasPermission(cmd.Item3);

            if (!hasPerm && sendMsg)
                client.SendChatMessage("Server", "You do not have permission to use this command!");

            return hasPerm;
        }

    }
}
