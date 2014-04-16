using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class EnvironmentUpdatePacket : Packet
    {

        public override byte PacketId
        {
            get
            {
                return 23;
            }
        }

        public byte[] Sky { get; set; }

        public byte[] Weather { get; set; }

        public EnvironmentUpdatePacket()
        {
            Sky = new byte[0];
            Weather = new byte[0];
        }

        public override void Read(IStarboundStream stream)
        {
            Sky = stream.ReadUInt8Array();
            Weather = stream.ReadUInt8Array();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt8Array(Sky);
            stream.WriteUInt8Array(Weather);
        }

    }
}
