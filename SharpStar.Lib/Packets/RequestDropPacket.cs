using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class RequestDropPacket : IPacket
    {
        public byte PacketId
        {
            get { return 28; }
        }

        public bool Ignore { get; set; }

        public long EntityId { get; set; }

        public void Read(IStarboundStream stream)
        {
            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
        }
    }
}