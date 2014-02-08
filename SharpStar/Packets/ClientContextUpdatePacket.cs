using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class ClientContextUpdatePacket : ServerPacket
    {
        public override byte PacketId
        {
            get
            {
                return 11;
            }
            set
            {
            }
        }

        public override bool Ignore { get; set; }

        public byte[] Unknown { get; set; }

        public ClientContextUpdatePacket()
        {
            Unknown = new byte[0];
        }

        public override void Read(StarboundStream stream)
        {
            Unknown = stream.ReadUInt8Array();
        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteUInt8Array(Unknown);
        }
    }
}
