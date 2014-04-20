using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Misc
{
    public class LogSector
    {

        public string SectorName { get; set; }

        public bool Unknown { get; set; }

        public LogSector()
        {
        }

        public LogSector(string name, bool unknown)
        {
            SectorName = name;
            Unknown = unknown;
        }

    }
}
