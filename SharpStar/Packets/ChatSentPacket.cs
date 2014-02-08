using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class ChatSentPacket : ClientPacket
    {

        public override byte PacketId
        {
            get
            {
                return 10;
            }
            set
            {
            }
        }

        public override bool Ignore { get; set; }

        public string Message { get; set; }

        public byte Channel { get; set; }
        
        public override void Read(StarboundStream stream)
        {
            Message = stream.ReadString();
            Channel = stream.ReadUInt8();
        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteString(Message);
            stream.WriteUInt8(Channel);
        }
    }
}
