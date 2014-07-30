using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.DataTypes
{
    public class Vec2F
    {

        public float X { get; set; }

        public float Y { get; set; }

        public static Vec2F FromStream(IStarboundStream stream)
        {
            Vec2F vec = new Vec2F();

            vec.X = stream.ReadSingle();
            vec.Y = stream.ReadSingle();

            return vec;
        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteSingle(X);
            stream.WriteSingle(Y);
        }

    }
}
