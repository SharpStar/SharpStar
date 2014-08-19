using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpStar.Lib.Packets;

namespace SharpStar.Lib.Plugins
{
    public class AfterPacketEventAttribute : PacketEventAttribute
    {
        public AfterPacketEventAttribute(params KnownPacket[] pType)
            : base(pType)
        {
        }
    }
}
