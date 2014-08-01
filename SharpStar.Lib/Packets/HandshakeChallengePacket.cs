﻿// SharpStar
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
    public class HandshakeChallengePacket : Packet
    {
        public override byte PacketId
        {
            get { return 3; }
        }

        public string Claim { get; set; }

        public string Salt { get; set; }

        public int Rounds { get; set; }

        public HandshakeChallengePacket()
        {
            Claim = String.Empty;
            Salt = String.Empty;
            Rounds = 5000;
        }

        public override void Read(IStarboundStream stream)
        {
            Claim = stream.ReadString();
            Salt = stream.ReadString();
            Rounds = stream.ReadInt32();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteString(Claim);
            stream.WriteString(Salt);
            stream.WriteInt32(Rounds);
        }
    }
}