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
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class HandshakeResponsePacket : Packet
    {

        public override byte PacketId
        {
            get
            {
                return 9;
            }
        }

        public string ClaimMessage { get; set; }
        public string PasswordHash { get; set; }


        public HandshakeResponsePacket()
        {
            ClaimMessage = String.Empty;
        }

        public HandshakeResponsePacket(string passwordHash)
            : this()
        {
            PasswordHash = passwordHash;
        }

        public override void Read(IStarboundStream stream)
        {
            ClaimMessage = stream.ReadString();
            PasswordHash = stream.ReadString();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteString(ClaimMessage);
            stream.WriteString(PasswordHash);
        }

    }
}
