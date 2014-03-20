using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class ChatSentPacket : IPacket
    {
        public byte PacketId
        {
            get { return 11; }
        }

        public bool Ignore { get; set; }

        public string Message { get; set; }

        public byte Channel { get; set; }

        public void Read(IStarboundStream stream)
        {
            Message = stream.ReadString();
            Channel = stream.ReadUInt8();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteString(Message);
            stream.WriteUInt8(Channel);
        }
    }
}