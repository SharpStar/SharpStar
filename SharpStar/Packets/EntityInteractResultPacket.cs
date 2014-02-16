using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;
using SharpStar.DataTypes;

namespace SharpStar.Packets
{
    public class EntityInteractResultPacket : ServerPacket
    {
        public override byte PacketId
        {
            get
            {
                return 22;
            }
            set
            {
            }
        }
        public override bool Ignore { get; set; }

        public uint ClientId { get; set; }

        public int EntityId { get; set; }

        public Variant Results { get; set; }

        public override void Read(StarboundStream stream)
        {
            ClientId = stream.ReadUInt32();
            EntityId = stream.ReadInt32();
            Results = stream.ReadVariant();
        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteUInt32(ClientId);
            stream.WriteInt32(EntityId);
            stream.WriteVariant(Results);
        }
    }
}
