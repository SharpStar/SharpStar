using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class WorldStopPacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 15;
            }
        }

        public bool Ignore { get; set; }


        public string Status { get; set; }

        public WorldStopPacket()
        {
            Status = String.Empty;
        }

        public void Read(StarboundStream stream)
        {
            Status = stream.ReadString();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteString(Status);
        }
    }
}
