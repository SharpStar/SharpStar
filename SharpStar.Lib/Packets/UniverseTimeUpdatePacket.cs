using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class UniverseTimeUpdatePacket : Packet
    {
        public override byte PacketId
        {
            get { return 5; }
        }

        private long Time { get; set; }

        public UniverseTimeUpdatePacket()
        {
            Time = 0L;
        }

        public override void Read(IStarboundStream stream)
        {
            int discarded;
            Time = stream.ReadSignedVLQ(out discarded);
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(Time);
        }
    }
}