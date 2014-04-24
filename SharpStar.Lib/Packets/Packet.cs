using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class Packet : IPacket
    {

        private byte _packetId;

        public virtual byte PacketId
        {
            get
            {
                return byte.MaxValue;
            }
            set
            {
                _packetId = value;
            }
        }
        
        public virtual bool Ignore { get; set; }

        public bool IsReceive { get; set; }

        public Packet()
        {
        }

        public virtual void Read(IStarboundStream stream)
        {
        }

        public virtual void Write(IStarboundStream stream)
        {
        }
    }
}
