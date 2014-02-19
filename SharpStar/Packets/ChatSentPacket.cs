using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class ChatSentPacket : IPacket
    {

        public byte PacketId
        {
            get
            {
                return 11;
            }
        }

        public bool Ignore { get; set; }

        public string Message { get; set; }

        public byte Channel { get; set; }
        
        public void Read(StarboundStream stream)
        {
            Message = stream.ReadString();
            Channel = stream.ReadUInt8();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteString(Message);
            stream.WriteUInt8(Channel);
        }
    }
}
