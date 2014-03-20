using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class EntityDestroyPacket : IPacket
    {
        public byte PacketId
        {
            get { return 44; }
            set { }
        }

        public bool Ignore { get; set; }

        public long EntityId { get; set; }

        public bool Death { get; set; }

        public void Read(IStarboundStream stream)
        {
            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
            Death = stream.ReadBoolean();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteBoolean(Death);
        }
    }
}