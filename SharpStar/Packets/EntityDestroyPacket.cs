using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class EntityDestroyPacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 42;
            }
            set
            {
            }
        }

        public bool Ignore { get; set; }

        public long EntityId { get; set; }

        public bool Death { get; set; }

        public void Read(StarboundStream stream)
        {

            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);
            Death = stream.ReadBoolean();

        }

        public void Write(StarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteBoolean(Death);
        }
    }
}
