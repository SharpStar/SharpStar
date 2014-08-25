using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Misc
{
    public class Sector : IWriteable
    {

        public string SectorId { get; set; }

        public string SectorName { get; set; }

        public double SectorSeed { get; set; }

        public string SectorPrefix { get; set; }

        public Variant Parameters { get; set; }

        public Variant SectorConfig { get; set; }

        public static Sector FromStream(IStarboundStream stream)
        {
            Sector sector = new Sector();
            sector.SectorId = stream.ReadString();
            sector.SectorName = stream.ReadString();
            sector.SectorSeed = stream.ReadDouble();
            sector.SectorPrefix = stream.ReadString();
            sector.Parameters = stream.ReadVariant();
            sector.SectorConfig = stream.ReadVariant();

            return sector;
        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteString(SectorId);
            stream.WriteString(SectorName);
            stream.WriteDouble(SectorSeed);
            stream.WriteString(SectorPrefix);
            stream.WriteVariant(Parameters);
            stream.WriteVariant(SectorConfig);
        }
    }
}
