using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class ClientContextUpdatePacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 11;
            }
            set
            {
            }
        }

        public bool Ignore { get; set; }

        public byte[] Unknown { get; set; }

        public ClientContextUpdatePacket()
        {
            Unknown = new byte[0];
        }

        public void Read(StarboundStream stream)
        {
            Unknown = stream.ReadUInt8Array();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteUInt8Array(Unknown);
        }
    }
}
