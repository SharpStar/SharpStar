using System;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class HandshakeChallengePacket : Packet
    {
        public override byte PacketId
        {
            get { return 3; }
        }

        public string Claim { get; set; }

        public string Salt { get; set; }

        public int Rounds { get; set; }

        public HandshakeChallengePacket()
        {
            Claim = String.Empty;
            Salt = String.Empty;
            Rounds = 5000;
        }

        public override void Read(IStarboundStream stream)
        {
            Claim = stream.ReadString();
            Salt = stream.ReadString();
            Rounds = stream.ReadInt32();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteString(Claim);
            stream.WriteString(Salt);
            stream.WriteInt32(Rounds);
        }
    }
}