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
    public class ChatSentPacket : Packet
    {
        public override byte PacketId
        {
            get { return 11; }
        }

        public string Message { get; set; }

        public byte Channel { get; set; }

        public override void Read(IStarboundStream stream)
        {
            Message = stream.ReadString();
            Channel = stream.ReadUInt8();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteString(Message);
            stream.WriteUInt8(Channel);
        }
    }
}