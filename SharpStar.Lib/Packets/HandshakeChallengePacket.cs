using System;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class HandshakeChallengePacket : IPacket
    {
        public byte PacketId
        {
            get { return 3; }
        }

        public bool Ignore { get; set; }

        public string Claim { get; set; }

        public string Salt { get; set; }

        public int Rounds { get; set; }

        public HandshakeChallengePacket()
        {
            Claim = String.Empty;
            Salt = String.Empty;
            Rounds = 5000;
        }

        public void Read(IStarboundStream stream)
        {
            Claim = stream.ReadString();
            Salt = stream.ReadString();
            Rounds = stream.ReadInt32();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteString(Claim);
            stream.WriteString(Salt);
            stream.WriteInt32(Rounds);
        }
    }
}