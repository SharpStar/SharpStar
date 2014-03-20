using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class ClientContextUpdatePacket : IPacket
    {
        public byte PacketId
        {
            get { return 13; }
        }

        public bool Ignore { get; set; }

        public byte[] Unknown { get; set; }

        public ClientContextUpdatePacket()
        {
            Unknown = new byte[0];
        }

        public void Read(IStarboundStream stream)
        {
            Unknown = stream.ReadUInt8Array();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteUInt8Array(Unknown);
        }
    }
}