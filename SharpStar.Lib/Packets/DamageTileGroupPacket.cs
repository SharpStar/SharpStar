using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Networking;
using SharpStar.Lib.DataTypes;

namespace SharpStar.Lib.Packets
{
    public class DamageTileGroupPacket : Packet
    {

        public List<Vec2I> Position { get; set; }

        public TileLayer Layer { get; set; }

        public Vec2F SourcePos { get; set; }

        public TileDamage TileDamage { get; set; }

        public override byte PacketId
        {
            get { return 27; }
        }

        public override void Read(IStarboundStream stream)
        {
            int discarded;
            ulong vlq = stream.ReadVLQ(out discarded);

            Position = new List<Vec2I>();

            for (int i = 0; i < (int)vlq; i++)
            {
                Vec2I vec = Vec2I.FromStream(stream);

                Position.Add(vec);
            }

            Layer = (TileLayer)stream.ReadUInt8();
            SourcePos = Vec2F.FromStream(stream);
            TileDamage = TileDamage.FromStream(stream);
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteVLQ((ulong)Position.Count);
            Position.ForEach(p => p.WriteTo(stream));
            stream.WriteUInt8((byte)Layer);
            SourcePos.WriteTo(stream);
            TileDamage.WriteTo(stream);
        }

    }
}
