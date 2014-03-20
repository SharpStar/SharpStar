using System;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class WorldStopPacket : IPacket
    {
        public byte PacketId
        {
            get { return 15; }
        }

        public bool Ignore { get; set; }


        public string Status { get; set; }

        public WorldStopPacket()
        {
            Status = String.Empty;
        }

        public void Read(IStarboundStream stream)
        {
            Status = stream.ReadString();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteString(Status);
        }
    }
}