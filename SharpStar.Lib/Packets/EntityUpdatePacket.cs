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

        public byte[] Unknown { get; set; }

        public override void Read(IStarboundStream stream)
        {
            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
            Unknown = stream.ReadUInt8Array((int)(stream.Length - stream.Position));
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteUInt8Array(Unknown, false);
        }
    }
}