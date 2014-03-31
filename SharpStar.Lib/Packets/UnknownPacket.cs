using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public sealed class UnknownPacket : Packet
    {

        public override byte PacketId { get; set; }

        public bool Compressed { get; set; }
        public byte[] Data { get; set; }
        private int Length { get; set; }

        public UnknownPacket()
        {
            PacketId = byte.MaxValue;
        }

        public UnknownPacket(bool compressed, int length, byte packetId)
        {
            Compressed = compressed;
            Length = length;
            Data = new byte[Length];
            PacketId = packetId;
            Ignore = false;
        }

        public override void Read(IStarboundStream stream)
        {
            stream.Read(Data, 0, Data.Length);
        }

        public override void Write(IStarboundStream stream)
        {
            stream.Write(Data, 0, Data.Length);
        }
    }
}