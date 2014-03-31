using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class EntityDestroyPacket : Packet
    {
        public override byte PacketId
        {
            get { return 44; }
        }

        public long EntityId { get; set; }

        public bool Death { get; set; }

        public byte[] Unknown { get; set; }

        public override void Read(IStarboundStream stream)
        {
            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
            Death = stream.ReadBoolean();

            Unknown = new byte[stream.Length - stream.Position];

            stream.Read(Unknown, 0, (int)(stream.Length - stream.Position));

        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteBoolean(Death);
            stream.Write(Unknown, 0, Unknown.Length);
        }
    }
}