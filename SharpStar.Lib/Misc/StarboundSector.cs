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
