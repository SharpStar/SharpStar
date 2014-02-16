using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class HandshakeChallengePacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 3;
            }
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
        
        public void Read(StarboundStream stream)
        {
            Claim = stream.ReadString();
            Salt = stream.ReadString();
            Rounds = stream.ReadInt32();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteString(Claim);
            stream.WriteString(Salt);
            stream.WriteInt32(Rounds);
        }
    }
}
