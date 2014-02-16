using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class UniverseTimeUpdatePacket : IPacket
    {

        public byte PacketId
        {
            get
            {
                return 5;
            }
        }


        public bool Ignore { get; set; }

        private long Time { get; set; }

        public UniverseTimeUpdatePacket()
        {
            Time = 0L;
        }

        public void Read(StarboundStream stream)
        {
            int discarded;
            Time = stream.ReadSignedVLQ(out discarded);
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteSignedVLQ(Time);
        }
    }
}
