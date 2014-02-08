using System;
using System.Collections.Generic;
using SharpStar.DataTypes;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class GiveItemPacket : ServerPacket
    {
        public override byte PacketId
        {
            get
            {
                return 19;
            }
            set
            {
            }
        }

        public override bool Ignore { get; set; }

        public string ItemName { get; set; }

        public ulong Count { get; set; }

        public Dictionary<string, Variant> ItemProperties { get; set; }

        public GiveItemPacket()
        {
            ItemName = String.Empty;
            ItemProperties = new Dictionary<string, Variant>();
        }

        public override void Read(StarboundStream stream)
        {

            int discarded;

            ItemName = stream.ReadString();
            Count = stream.ReadVLQ(out discarded);
            ItemProperties = (Dictionary<string, Variant>)stream.ReadVariant().Value;

        }

        public override void Write(StarboundStream stream)
        {
            stream.WriteString(ItemName);
            stream.WriteVLQ(Count);
            stream.WriteVariant(new Variant(ItemProperties));
        }
    }
}
