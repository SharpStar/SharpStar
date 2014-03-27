using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.DataTypes;

namespace SharpStar.Lib.Starbound
{
    public class Metadata
    {

        public Variant Data { get; set; }

        public int Version { get; set; }

        public Metadata()
        {
        }

        public Metadata(Variant data, int version)
        {
            Data = data;
            Version = version;
        }

    }
}
