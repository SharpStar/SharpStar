// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
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

        public static bool IsAdmin(this SharpStarClient client)
        {
            return client.Server.Player.UserAccount != null && client.Server.Player.UserAccount.IsAdmin;
        }

        public static bool CanUserAccess(this SharpStarClient client, string command, bool sendMsg = false)
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
