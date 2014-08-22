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
using Mono;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Starbound
{
    public class World : BTreeDb4
    {

        public const int TilesX = 32;
        public const int TilesY = 32;

        public const int TilesPerRegion = TilesX * TilesY;

        public Metadata Metadata { get; set; }


        public override void Read(byte[] data)
        {

            base.Read(data);

            if (Identifier != "World2")
                throw new Exception("Expected the identifier 'World2'");

        }

        public List<Document> GetEntities(byte x, byte y)
        {

            byte[] data = Get(new byte[] { 2, x, y });

            return Document.ListFromStream(data);

        }

        public List<Tile> GetTiles(byte x, byte y)
        {

            byte[] data = Get(new byte[] { 1, x, y });

            var tiles = new List<Tile>();

            using (MemoryStream ms = new MemoryStream(data))
            {

                byte[] unknown = new byte[3];

                ms.Read(unknown, 0, unknown.Length);

                for (int i = 0; i < TilesPerRegion; i++)
                {
                    tiles.Add(Tile.FromStream(ms));
                }

            }

            return tiles;

        }

        public Metadata GetMetadata()
        {

            if (Metadata != null)
                return Metadata;

            using (StarboundStream ss = new StarboundStream(GetRaw(new byte[] { 0, 0, 0 })))
            {

                var unpacked = DataConverter.Unpack("^ii", ss.ReadUInt8Array(8), 0); //unknown

                Document doc = Document.FromStream(ss);

                if (doc.Name != "WorldMetadata")
                    throw new Exception("Invalid world data!");

                Metadata = new Metadata(doc.Data, doc.Version);

                return Metadata;

            }

        }

        public override byte[] EncodeKey(byte[] key)
        {
            return DataConverter.PackEnumerable("^CSS", key);
        }

    }
}
