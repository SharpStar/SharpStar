using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class UniverseTimeUpdatePacket : IPacket
    {
        public byte PacketId
        {
            get { return 5; }
        }


        public bool Ignore { get; set; }

        private long Time { get; set; }

        public UniverseTimeUpdatePacket()
        {
            Time = 0L;
        }

        public void Read(IStarboundStream stream)
        {
            int discarded;
            Time = stream.ReadSignedVLQ(out discarded);
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(Time);
        }
    }
}