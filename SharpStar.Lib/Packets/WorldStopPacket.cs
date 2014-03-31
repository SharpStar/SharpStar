using System;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class WorldStopPacket : Packet
    {

        public override byte PacketId
        {
            get { return 15; }
        }

        public string Status { get; set; }

        public WorldStopPacket()
        {
            Status = String.Empty;
        }

        public override void Read(IStarboundStream stream)
        {
            Status = stream.ReadString();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteString(Status);
        }
    }
}