using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class EntityUpdatePacket : ServerPacket
    {
        public override byte PacketId
        {
            get
            {
                return 41;
            }
            set
            {
            }
        }
        public override bool Ignore { get; set; }

        public long EntityId { get; set; }

        public byte[] Delta { get; set; }

        public override void Read(StarboundStream stream)
        {

            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
            Delta = stream.ReadUInt8Array();

        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteUInt8Array(Delta);
        }
    }
}
