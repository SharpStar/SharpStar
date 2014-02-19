using SharpStar.Networking;

namespace SharpStar.Packets
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet)
    public class ConnectionResponsePacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 1;
            }
        }

        public bool Ignore { get; set; }

        public bool Success { get; set; }

        public ulong ClientId { get; set; }

        public string RejectionReason { get; set; }

        public byte[] Unknown { get; set; }


        public void Read(StarboundStream stream)
        {

            int discarded;
            Success = stream.ReadBoolean();
            ClientId = stream.ReadVLQ(out discarded);
            RejectionReason = stream.ReadString();

            Unknown = new byte[stream.Length - stream.Position];

            stream.Read(Unknown, 0, (int)(stream.Length - stream.Position));

        }

        public void Write(StarboundStream stream)
        {
            stream.WriteBoolean(Success);
            stream.WriteVLQ(ClientId);
            stream.WriteString(RejectionReason);
            stream.Write(Unknown, 0, Unknown.Length);
        }
    }
}
