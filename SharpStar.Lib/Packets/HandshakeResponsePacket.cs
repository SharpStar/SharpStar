using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class HandshakeResponsePacket : Packet
    {

        public override byte PacketId
        {
            get
            {
                return 9;
            }
        }

        public string ClaimMessage { get; set; }
        public string PasswordHash { get; set; }


        public HandshakeResponsePacket()
        {
            ClaimMessage = String.Empty;
        }

        public HandshakeResponsePacket(string passwordHash)
            : this()
        {
            PasswordHash = passwordHash;
        }

        public override void Read(IStarboundStream stream)
        {
            ClaimMessage = stream.ReadString();
            PasswordHash = stream.ReadString();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteString(ClaimMessage);
            stream.WriteString(PasswordHash);
        }

    }
}
