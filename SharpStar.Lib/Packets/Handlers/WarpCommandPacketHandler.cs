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
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class WarpCommandPacketHandler : PacketHandler<WarpCommandPacket>
    {
        public override void Handle(WarpCommandPacket packet, StarboundClient client)
        {

            if (packet.WarpType == WarpType.WarpHome)
            {
                client.Server.Player.PlayerShip = null;
                client.Server.Player.OnOwnShip = true;
            }
            else if (packet.WarpType == WarpType.WarpOtherShip)
            {
                client.Server.Player.PlayerShip = packet.Player;
                client.Server.Player.OnOwnShip = false;
            }
            else
            {
                client.Server.Player.OnOwnShip = false;
            }

            SharpStarMain.Instance.PluginManager.CallEvent("warpCommand", packet, client);
        
        }

        public override void HandleAfter(WarpCommandPacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterWarpCommand", packet, client);
        }
    }
}