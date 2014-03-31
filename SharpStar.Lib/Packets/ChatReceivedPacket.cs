﻿using System;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class ChatReceivedPacket : Packet
    {

        public override byte PacketId
        {
            get { return 4; }
        }
        
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

        public override void Read(IStarboundStream stream)
        {
            Channel = stream.ReadUInt8();
            World = stream.ReadString();
            ClientId = stream.ReadUInt32();
            Name = stream.ReadString();
            Message = stream.ReadString();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt8(Channel);
            stream.WriteString(World);
            stream.WriteUInt32(ClientId);
            stream.WriteString(Name);
            stream.WriteString(Message);
        }
    }
}