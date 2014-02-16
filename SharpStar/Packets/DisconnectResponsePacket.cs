using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class DisconnectResponsePacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 2;
            }        }
        
        public bool Ignore { get; set; }

        public byte Unknown { get; set; }

        public DisconnectResponsePacket()
        {
            Unknown = 0;
        }

        public void Read(StarboundStream stream)
        {
            Unknown = stream.ReadUInt8();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteUInt8(Unknown);
        }
    }
}
