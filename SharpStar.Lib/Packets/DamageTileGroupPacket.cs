// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Networking;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Tiles;

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
            get { return (byte)KnownPacket.DamageTileGroup; }
        }

        public override void Read(IStarboundStream stream)
        {
            ulong vlq = stream.ReadVLQ();

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
