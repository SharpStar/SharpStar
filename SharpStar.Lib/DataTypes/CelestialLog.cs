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

namespace SharpStar.Lib.DataTypes
{
    public class CelestialLog
    {

        public List<LogSector> Sectors { get; set; }

        public List<SystemCoordinate> Visited { get; set; }

        public SystemCoordinate CurrentSystem { get; set; }

        public WorldCoordinate CurrentLocation { get; set; }

        public WorldCoordinate HomeCoordinate { get; set; }

        public CelestialLog()
        {
            Sectors = new List<LogSector>();
            Visited = new List<SystemCoordinate>();
            CurrentLocation = new WorldCoordinate();
            HomeCoordinate = new WorldCoordinate();
        }

        public static CelestialLog FromStream(IStarboundStream stream)
        {

            CelestialLog log = new CelestialLog();

            byte[] logDat = stream.ReadUInt8Array();

            using (StarboundStream s = new StarboundStream(logDat))
            {

                uint visited = s.ReadUInt32();

                for (int i = 0; i < visited; i++)
                {
                    log.Visited.Add(s.ReadSystemCoordinate());
                }

                uint sectors = s.ReadUInt32();

                for (int i = 0; i < sectors; i++)
                {
                    log.Sectors.Add(new LogSector { SectorName = s.ReadString(), Unknown = s.ReadBoolean() });
                }

                s.ReadUInt8(); //unknown

                log.CurrentSystem = s.ReadSystemCoordinate();
                log.CurrentLocation = s.ReadWorldCoordinate();
                log.HomeCoordinate = s.ReadWorldCoordinate();

            }

            return log;

        }

        public void WriteTo(IStarboundStream stream)
        {
        }

    }
}
