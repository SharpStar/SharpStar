using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class OpenContainerPacket : Packet
    {
        public override byte PacketId
        {
            get { return 33; }
        }

        public long EntityId { get; set; }

        public override void Read(IStarboundStream stream)
        {
            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
        }
    }
}