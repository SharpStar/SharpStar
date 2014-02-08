using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class UniverseTimeUpdatePacket : ServerPacket
    {

        public override byte PacketId
        {
            get
            {
                return 5;
            }
            set
            {
            }
        }


        public override bool Ignore { get; set; }

        private long Time { get; set; }

        public UniverseTimeUpdatePacket()
        {
            Time = 0L;
        }

        public override void Read(StarboundStream stream)
        {
            int discarded;
            Time = stream.ReadSignedVLQ(out discarded);
        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteSignedVLQ(Time);
        }
    }
}
