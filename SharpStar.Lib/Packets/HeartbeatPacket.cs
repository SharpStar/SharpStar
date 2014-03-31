using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class HeartbeatPacket : Packet
    {
        public override byte PacketId
        {
            get { return 48; }
        }

        public ulong CurrentStep { get; set; }

        public override void Read(IStarboundStream stream)
        {
            int discarded;

            CurrentStep = stream.ReadVLQ(out discarded);
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteVLQ(CurrentStep);
        }
    }
}