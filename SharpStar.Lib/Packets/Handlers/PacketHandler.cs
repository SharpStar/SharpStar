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
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public abstract class PacketHandler<T> : IPacketHandler where T : Packet, new()
    {
        public abstract void Handle(T packet, SharpStarClient client);

        public abstract void HandleAfter(T packet, SharpStarClient client);

        private static readonly int _packetId = new T().PacketId;

        public int PacketId
        {
            get
            {
                return _packetId;
            }
        }

        public Type GetPacketType()
        {
            return typeof (T);
        }

        public void Handle(IPacket packet, IClient client)
        {
            if (packet is T)
                Handle((T) packet, (SharpStarClient) client);
            else
                throw new Exception(String.Format("Was given packet type {0}, expected {1}", packet.GetType().Name,
                    typeof (T).Name));
        }

        public void HandleAfter(IPacket packet, IClient client)
        {
            if (packet is T)
                HandleAfter((T) packet, (SharpStarClient) client);
            else
                throw new Exception(String.Format("Was given packet type {0}, expected {1}", packet.GetType().Name,
                    typeof (T).Name));
        }

    }
}