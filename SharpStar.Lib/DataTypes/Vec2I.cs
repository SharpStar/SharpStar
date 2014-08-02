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
    /// Contains X, Y coordinates as integers
    /// </summary>
    public class Vec2I : IWriteable
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
