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
