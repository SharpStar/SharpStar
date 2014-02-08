using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class DisconnectResponsePacket : ServerPacket
    {
        public override byte PacketId
        {
            get
            {
                return 2;
            }
            set
            {
            }
        }
        
        public override bool Ignore { get; set; }

        public byte Unknown { get; set; }

        public DisconnectResponsePacket()
        {
            Unknown = 0;
        }

        public override void Read(StarboundStream stream)
        {
            Unknown = stream.ReadUInt8();
        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteUInt8(Unknown);
        }
    }
}
