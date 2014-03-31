using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class ChatSentPacket : Packet
    {
        public override byte PacketId
        {
            get { return 11; }
        }

        public string Message { get; set; }

        public byte Channel { get; set; }

        public override void Read(IStarboundStream stream)
        {
            Message = stream.ReadString();
            Channel = stream.ReadUInt8();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteString(Message);
            stream.WriteUInt8(Channel);
        }
    }
}