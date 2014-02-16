using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class DamageNotificationPacket : IPacket
    {

        public byte PacketId
        {
            get
            {
                return 43;
            }
        }

        public bool Ignore { get; set; }

        public long CauseEntityId { get; set; }

        public long TargetEntityId { get; set; }

        public long PositionX { get; set; }

        public long PositionY { get; set; }

        public long Damage { get; set; }

        public byte DamageKind { get; set; }

        public string DamageSourceKind { get; set; }

        public string TargetMaterialKind { get; set; }

        public byte HitResultKind { get; set; }

        public void Read(StarboundStream stream)
        {

            int discarded;

            CauseEntityId = stream.ReadSignedVLQ(out discarded);
            TargetEntityId = stream.ReadSignedVLQ(out discarded);
            PositionX = stream.ReadSignedVLQ(out discarded) / 100;
            PositionY = stream.ReadSignedVLQ(out discarded) / 100;
            Damage = stream.ReadSignedVLQ(out discarded) / 100;
            DamageKind = stream.ReadUInt8();
            DamageSourceKind = stream.ReadString();
            TargetMaterialKind = stream.ReadString();
            HitResultKind = stream.ReadUInt8();

        }

        public void Write(StarboundStream stream)
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
