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
using System.Collections.Generic;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet)
    public class ConnectionResponsePacket : Packet
    {
        public override byte PacketId
        {
            get { return (byte)KnownPacket.ConnectionResponse; }
        }

        public bool Success { get; set; }

        public ulong ClientId { get; set; }

        public string RejectionReason { get; set; }

        public List<CelestialInfo> CelestialInfos { get; set; }

        public ConnectionResponsePacket()
        {
            CelestialInfos = new List<CelestialInfo>();
        }

        public override void Read(IStarboundStream stream)
        {
            Success = stream.ReadBoolean();
            ClientId = stream.ReadVLQ();
            RejectionReason = stream.ReadString();
            CelestialInfos = new List<CelestialInfo>();

            ulong length = stream.ReadVLQ();

            for (ulong i = 0; i < length; i++)
            {
                CelestialInfos.Add(CelestialInfo.FromStream(stream));
            }
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteBoolean(Success);
            stream.WriteVLQ(ClientId);
            stream.WriteString(RejectionReason);
            stream.WriteVLQ((ulong)CelestialInfos.Count);

            foreach (CelestialInfo cInfo in CelestialInfos)
            {
                cInfo.WriteTo(stream);
            }
        }
    }
}