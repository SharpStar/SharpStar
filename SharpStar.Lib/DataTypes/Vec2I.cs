using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.DataTypes
{
    public class Vec2I
    {

        public int X { get; set; }

        public int Y { get; set; }

        public static Vec2I FromStream(IStarboundStream stream)
        {
            Vec2I vec = new Vec2I();
            vec.X = stream.ReadInt32();
            vec.Y = stream.ReadInt32();

            return vec;
        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteInt32(X);
            stream.WriteInt32(Y);
        }

    }
}
