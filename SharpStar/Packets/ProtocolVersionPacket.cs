using SharpStar.Networking;

namespace SharpStar.Packets
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet)
    public class ProtocolVersionPacket : IPacket
    {

        public byte PacketId
        {
            get
            {
                return 0;
            }
        }
        public bool Ignore { get; set; }

        public uint ProtocolVersion { get; set; }

        public ProtocolVersionPacket()
        {
        }

        public ProtocolVersionPacket(uint protocolVersion)
        {
            ProtocolVersion = protocolVersion;
            Ignore = false;
        }

        public void Read(StarboundStream stream)
        {
            ProtocolVersion = stream.ReadUInt32();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteUInt32(ProtocolVersion);
        }

    }
}
