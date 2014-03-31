using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class DamageNotificationPacket : Packet
    {
        public override byte PacketId
        {
            get { return 45; }
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
            int discarded;

            CauseEntityId = stream.ReadSignedVLQ(out discarded);
            TargetEntityId = stream.ReadSignedVLQ(out discarded);
            PositionX = stream.ReadSignedVLQ(out discarded)/100;
            PositionY = stream.ReadSignedVLQ(out discarded)/100;
            Damage = stream.ReadSignedVLQ(out discarded)/100;
            DamageKind = stream.ReadUInt8();
            DamageSourceKind = stream.ReadString();
            TargetMaterialKind = stream.ReadString();
            HitResultKind = stream.ReadUInt8();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(CauseEntityId);
            stream.WriteSignedVLQ(TargetEntityId);
            stream.WriteSignedVLQ(PositionX*100);
            stream.WriteSignedVLQ(PositionY*100);
            stream.WriteSignedVLQ(Damage*100);
            stream.WriteUInt8(DamageKind);
            stream.WriteString(DamageSourceKind);
            stream.WriteString(TargetMaterialKind);
            stream.WriteUInt8(HitResultKind);
        }
    }
}