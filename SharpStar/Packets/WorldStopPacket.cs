using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class WorldStopPacket : ServerPacket
    {
        public override byte PacketId
        {
            get
            {
                return 13;
            }
            set
            {
            }
        }

        public override bool Ignore { get; set; }


        public string Status { get; set; }

        public WorldStopPacket()
        {
            Status = String.Empty;
        }

        public override void Read(StarboundStream stream)
        {
            Status = stream.ReadString();
        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteString(Status);
        }
    }
}
