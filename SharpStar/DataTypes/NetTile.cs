using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;

namespace SharpStar.DataTypes
{
    public class NetTile
    {

        public short Unknown0 { get; set; }

        public byte Unknown1 { get; set; }

        public byte Unknown2 { get; set; }

        public short Unknown3 { get; set; }

        public byte Unknown4 { get; set; }

        public short Unknown5 { get; set; }

        public byte Unknown6 { get; set; }

        public byte Unknown7 { get; set; }

        public short Unknown8 { get; set; }

        public byte Unknown9 { get; set; }

        public byte Unknown10 { get; set; }

        public byte Unknown11 { get; set; }

        public byte Unknown12 { get; set; }

        public byte Unknown13 { get; set; }

        public byte LiquidLevel { get; set; }

        public long Gravity { get; set; }

        public NetTile()
        {
        }

    }
}
