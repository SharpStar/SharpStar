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

using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Tiles
{
    public class TileDamage : IWriteable
    {

        public TileDamageType DamageType { get; set; }

        public float Amount { get; set; }

        public static TileDamage FromStream(IStarboundStream stream)
        {
            TileDamage dmg = new TileDamage();
            dmg.DamageType = (TileDamageType)stream.ReadUInt8();
            dmg.Amount = stream.ReadSingle();

            return dmg;
        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteUInt8((byte)DamageType);
            stream.WriteSingle(Amount);
        }

    }

    public enum TileDamageType : byte
    {
        None = 0,
        Plantish = 1,
        Blockish = 2,
        Beamish = 3,
        Explosive = 4,
        Fire = 5,
        Tilling = 6,
        Crushing = 7
    }

}
