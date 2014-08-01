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
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public sealed class UnknownPacket : Packet
    {

        public override byte PacketId { get; set; }

        public bool Compressed { get; set; }
        public byte[] Data { get; set; }
        private int Length { get; set; }

        public UnknownPacket()
        {
            PacketId = byte.MaxValue;
        }

        public UnknownPacket(bool compressed, int length, byte packetId)
        {
            Compressed = compressed;
            Length = length;
            Data = new byte[Length];
            PacketId = packetId;
            Ignore = false;
        }

        public override void Read(IStarboundStream stream)
        {
            stream.Read(Data, 0, Data.Length);
        }

        public override void Write(IStarboundStream stream)
        {
            stream.Write(Data, 0, Data.Length);
        }
    }
}