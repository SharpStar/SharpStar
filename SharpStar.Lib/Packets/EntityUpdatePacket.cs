using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class EntityUpdatePacket : IPacket
    {
        public byte PacketId
        {
            get { return 43; }
        }

        public bool Ignore { get; set; }

        public long EntityId { get; set; }

        public byte[] Delta { get; set; }

        public void Read(IStarboundStream stream)
        {
            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
            Delta = stream.ReadUInt8Array();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteUInt8Array(Delta);
        }
    }
}