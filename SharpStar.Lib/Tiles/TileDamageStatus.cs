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
    public class TileDamageStatus : IWriteable
    {

        public TileDamageParameters Parameters { get; set; }

        public Vec2F SourcePosition { get; set; }

        public TileDamage Damage { get; set; }

        public static TileDamageStatus FromStream(IStarboundStream stream)
        {
            TileDamageStatus status = new TileDamageStatus();
            status.Parameters = TileDamageParameters.FromStream(stream);
            status.SourcePosition = Vec2F.FromStream(stream);
            status.Damage = TileDamage.FromStream(stream);

            return status;
        }

        public void WriteTo(IStarboundStream stream)
        {
            Parameters.WriteTo(stream);
            SourcePosition.WriteTo(stream);
            Damage.WriteTo(stream);
        }

    }
}
