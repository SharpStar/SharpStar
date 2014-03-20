using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class ClientDisconnectPacket : IPacket
    {
        public byte PacketId
        {
            get { return 8; }
        }

        public bool Ignore { get; set; }

        public byte Unknown { get; set; }

        public ClientDisconnectPacket()
        {
            Unknown = 0;
        }

        public void Read(IStarboundStream stream)
        {
            Unknown = stream.ReadUInt8();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteUInt8(Unknown);
        }
    }
}