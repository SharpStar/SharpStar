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
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class EntityInteractResultPacket : Packet
    {
        public override byte PacketId
        {
            get { return 24; }
        }

        public uint ClientId { get; set; }

        public int EntityId { get; set; }

        public Variant Results { get; set; }

        public override void Read(IStarboundStream stream)
        {
            ClientId = stream.ReadUInt32();
            EntityId = stream.ReadInt32();
            Results = stream.ReadVariant();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt32(ClientId);
            stream.WriteInt32(EntityId);
            stream.WriteVariant(Results);
        }
    }
}