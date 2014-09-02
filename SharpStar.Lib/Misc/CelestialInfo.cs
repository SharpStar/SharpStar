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
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Misc
{
    public class CelestialInfo : IWriteable
    {
        public int OrbitalLevels { get; set; }

        public int ChunkSize { get; set; }

        public int XyCoordinateMin { get; set; }

        public int XyCoordinateMax { get; set; }

        public int ZCoordinateMin { get; set; }

        public int ZCoordinateMax { get; set; }

        public List<Sector> Sectors { get; set; }

        public CelestialInfo()
        {
            Sectors = new List<Sector>();
        }

        public static CelestialInfo FromStream(IStarboundStream stream)
        {
            CelestialInfo cInfo = new CelestialInfo();
            cInfo.OrbitalLevels = stream.ReadInt32();
            cInfo.ChunkSize = stream.ReadInt32();
            cInfo.XyCoordinateMin = stream.ReadInt32();
            cInfo.XyCoordinateMax = stream.ReadInt32();
            cInfo.ZCoordinateMin = stream.ReadInt32();
            cInfo.ZCoordinateMax = stream.ReadInt32();

            ulong length = stream.ReadVLQ();

            for (ulong i = 0; i < length; i++)
            {
                cInfo.Sectors.Add(Sector.FromStream(stream));
            }

            return cInfo;
        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteInt32(OrbitalLevels);
            stream.WriteInt32(ChunkSize);
            stream.WriteInt32(XyCoordinateMin);
            stream.WriteInt32(XyCoordinateMax);
            stream.WriteInt32(ZCoordinateMin);
            stream.WriteInt32(ZCoordinateMax);
            stream.WriteVLQ((ulong)Sectors.Count);

            foreach (Sector sector in Sectors)
            {
                sector.WriteTo(stream);
            }
        }
    }
}
