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
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class ChatReceivedPacket : Packet
    {

        public override byte PacketId
        {
            get { return 4; }
        }
        
        public byte Channel { get; set; }

        public string World { get; set; }

        public uint ClientId { get; set; }

        public string Name { get; set; }

        public string Message { get; set; }

        public ChatReceivedPacket()
        {
            World = String.Empty;
            Name = String.Empty;
            Message = String.Empty;
        }

        public override void Read(IStarboundStream stream)
        {
            Channel = stream.ReadUInt8();
            World = stream.ReadString();
            ClientId = stream.ReadUInt32();
            Name = stream.ReadString();
            Message = stream.ReadString();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt8(Channel);
            stream.WriteString(World);
            stream.WriteUInt32(ClientId);
            stream.WriteString(Name);
            stream.WriteString(Message);
        }
    }
}