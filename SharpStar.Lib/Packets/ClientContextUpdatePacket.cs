using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class ClientContextUpdatePacket : Packet
    {
        public override byte PacketId
        {
            get { return 13; }
        }

        public byte[] Unknown { get; set; }

        public ClientContextUpdatePacket()
        {
            Unknown = new byte[0];
        }

        public override void Read(IStarboundStream stream)
        {
            Unknown = stream.ReadUInt8Array();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt8Array(Unknown);
        }
    }
}