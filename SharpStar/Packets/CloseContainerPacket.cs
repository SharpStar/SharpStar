using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class CloseContainerPacket : ClientPacket
    {
        public override byte PacketId
        {
            get
            {
                return 32;
            }
            set
            {
            }
        }
        public override bool Ignore { get; set; }

        public long EntityId { get; set; }

        public override void Read(StarboundStream stream)
        {

            int discarded;

            EntityId = stream.ReadSignedVLQ(out discarded);

        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
        }
    }
}
