using System;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class ChatReceivedPacket : ServerPacket
    {
        public override byte PacketId
        {
            get { return 4; }
            set
            {
            }
        }

        public override bool Ignore { get; set; }

        public byte Channel { get; set; }

        public string World { get; set; }

        public uint ClientId { get; set; }

        public string Name { get; set; }

        public string Message { get; set; }

        public ChatReceivedPacket()
        {
            World = String.Empty;
            Name = String.Empty;
            Message = String.Empty;
        }

        public override void Read(StarboundStream stream)
        {
            Channel = stream.ReadUInt8();
            World = stream.ReadString();
            ClientId = stream.ReadUInt32();
            Name = stream.ReadString();
            Message = stream.ReadString();
        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteUInt8(Channel);
            stream.WriteString(World);
            stream.WriteUInt32(ClientId);
            stream.WriteString(Name);
            stream.WriteString(Message);
        }
    }
}
