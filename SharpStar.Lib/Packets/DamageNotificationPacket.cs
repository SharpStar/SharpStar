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
    public class DamageNotificationPacket : Packet
    {
        public override byte PacketId
        {
            get { return (byte)KnownPacket.DamageNotification; }
        }

        public long CauseEntityId { get; set; }

        public long TargetEntityId { get; set; }

        public long PositionX { get; set; }

        public long PositionY { get; set; }

        public long Damage { get; set; }

        public byte DamageKind { get; set; }

        public string DamageSourceKind { get; set; }

        public string TargetMaterialKind { get; set; }

        public byte HitResultKind { get; set; }

        public override void Read(IStarboundStream stream)
        {
            CauseEntityId = stream.ReadSignedVLQ();
            TargetEntityId = stream.ReadSignedVLQ();
            PositionX = stream.ReadSignedVLQ() / 100;
            PositionY = stream.ReadSignedVLQ() / 100;
            Damage = stream.ReadSignedVLQ() / 100;
            DamageKind = stream.ReadUInt8();
            DamageSourceKind = stream.ReadString();
            TargetMaterialKind = stream.ReadString();
            HitResultKind = stream.ReadUInt8();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(CauseEntityId);
            stream.WriteSignedVLQ(TargetEntityId);
            stream.WriteSignedVLQ(PositionX * 100);
            stream.WriteSignedVLQ(PositionY * 100);
            stream.WriteSignedVLQ(Damage * 100);
            stream.WriteUInt8(DamageKind);
            stream.WriteString(DamageSourceKind);
            stream.WriteString(TargetMaterialKind);
            stream.WriteUInt8(HitResultKind);
        }
    }
}