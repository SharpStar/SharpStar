using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class DisconnectResponsePacket : Packet
    {
        public override byte PacketId
        {
            get { return 2; }
        }

        public byte Unknown { get; set; }

        public DisconnectResponsePacket()
        {
            Unknown = 0;
        }

        public override void Read(IStarboundStream stream)
        {
            Unknown = stream.ReadUInt8();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt8(Unknown);
        }
    }
}