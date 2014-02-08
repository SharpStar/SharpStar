using SharpStar.Networking;

namespace SharpStar.Packets
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet)
    public class ConnectionResponsePacket : ServerPacket
    {
        public override byte PacketId
        {
            get
            {
                return 1;
            }
            set
            {
            }
        }
        public override bool Ignore { get; set; }

        public bool Success { get; set; }
        public ulong ClientId { get; set; }
        public string RejectionReason { get; set; }

        public override void Read(StarboundStream stream)
        {
            int discarded;
            Success = stream.ReadBoolean();
            ClientId = stream.ReadVLQ(out discarded);
            RejectionReason = stream.ReadString();
        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteBoolean(Success);
            stream.WriteVLQ(ClientId);
            stream.WriteString(RejectionReason);
        }
    }
}
