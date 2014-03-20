using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class HeartbeatPacket : IPacket
    {
        public byte PacketId
        {
            get { return 48; }
        }

        public bool Ignore { get; set; }

        public ulong CurrentStep { get; set; }

        public void Read(IStarboundStream stream)
        {
            int discarded;

            CurrentStep = stream.ReadVLQ(out discarded);
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteVLQ(CurrentStep);
        }
    }
}