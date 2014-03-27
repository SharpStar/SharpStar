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

            using (MemoryStream ms = new MemoryStream(GetRaw(new byte[] { 0, 0, 0 })))
            {

                using (StarboundStream ss = new StarboundStream(ms))
                {

                    var unpacked = DataConverter.Unpack("^ii", ss.ReadUInt8Array(8), 0); //unknown

                    Document doc = Document.FromStream(ss);

                    if (doc.Name != "WorldMetadata")
                        throw new Exception("Invalid world data!");

                    Metadata = new Metadata(doc.Data, doc.Version);

                    return Metadata;

                }

            }

        }

        public override byte[] EncodeKey(byte[] key)
        {
            return DataConverter.PackEnumerable("^CSS", key);
        }

    }
}
