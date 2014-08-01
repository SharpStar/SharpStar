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

namespace SharpStar.Lib.Misc
{
    public class StarboundSector
    {

        public static List<StarboundSector> Sectors
        {
            get
            {

                var sectors = new List<StarboundSector>();
                sectors.Add(FromName("alpha"));
                sectors.Add(FromName("beta"));
                sectors.Add(FromName("gamma"));
                sectors.Add(FromName("delta"));
                sectors.Add(FromName("sectorx"));

                return sectors;


            }
        }

        public string Name { get; set; }

        public byte[] Data { get; set; }

        public static StarboundSector FromName(string name)
        {

            byte[] data = Encoding.UTF8.GetBytes(name);

            byte[] buffer = new byte[data.Length + 1];
            
            buffer[0] = (byte)data.Length;
            
            Buffer.BlockCopy(data, 0, buffer, 1, data.Length);

            return new StarboundSector { Name = name, Data = data };


        }

    }
}
