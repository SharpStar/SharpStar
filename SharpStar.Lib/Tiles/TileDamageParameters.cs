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
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Tiles
{
    public class TileDamageParameters : IWriteable
    {

        public Variant DamageFactors { get; set; }

        public float TotalHealth { get; set; }

        public static TileDamageParameters FromStream(IStarboundStream stream)
        {
            TileDamageParameters parameters = new TileDamageParameters();
            parameters.DamageFactors = stream.ReadVariant();
            parameters.TotalHealth = stream.ReadSingle();

            return parameters;
        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteVariant(DamageFactors);
            stream.WriteSingle(TotalHealth);
        }
    }
}
