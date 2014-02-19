using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class HeartbeatPacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 48;
            }
        }

        public bool Ignore { get; set; }

        public ulong CurrentStep { get; set; }

        public void Read(StarboundStream stream)
        {

            int discarded;

            CurrentStep = stream.ReadVLQ(out discarded);
        
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteVLQ(CurrentStep);
        }
    }
}
