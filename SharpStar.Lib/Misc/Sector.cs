using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Misc
{
    public class Sector
    {

        public string SectorName { get; set; }

        public bool Unknown { get; set; }

        public Sector()
        {
        }

        public Sector(string name, bool unknown)
        {
            SectorName = name;
            Unknown = unknown;
        }

    }
}
