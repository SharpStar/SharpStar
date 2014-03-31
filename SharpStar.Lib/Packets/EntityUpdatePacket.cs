using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class EntityUpdatePacket : Packet
    {
        public override byte PacketId
        {
            get { return 43; }
        }

        public long EntityId { get; set; }

        public byte[] Delta { get; set; }

        public override void Read(IStarboundStream stream)
        {
            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
            Delta = stream.ReadUInt8Array();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteUInt8Array(Delta);
        }
    }
}