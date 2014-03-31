using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet)
    public class ProtocolVersionPacket : Packet
    {
        public override byte PacketId
        {
            get { return 0; }
        }

        public uint ProtocolVersion { get; set; }

        public ProtocolVersionPacket()
        {
        }

        public ProtocolVersionPacket(uint protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }

        public override void Read(IStarboundStream stream)
        {
            ProtocolVersion = stream.ReadUInt32();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt32(ProtocolVersion);
        }
    }
}