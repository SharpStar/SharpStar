using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class EntityUpdatePacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 43;
            }
        }

        public bool Ignore { get; set; }

        public long EntityId { get; set; }

        public byte[] Delta { get; set; }

        public void Read(StarboundStream stream)
        {

            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
            Delta = stream.ReadUInt8Array();

        }

        public void Write(StarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteUInt8Array(Delta);
        }
    }
}
