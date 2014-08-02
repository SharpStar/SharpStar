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
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.DataTypes
{
    /// <summary>
    /// Contains X, Y coordinates as floats
    /// </summary>
    public class Vec2F : IWriteable
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
