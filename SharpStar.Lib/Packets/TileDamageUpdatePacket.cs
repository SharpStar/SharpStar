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
using SharpStar.Lib.Misc;
using SharpStar.Lib.Networking;
using SharpStar.Lib.Tiles;

namespace SharpStar.Lib.Packets
{
    public class TileDamageUpdatePacket : Packet
    {
        public override byte PacketId
        {
            get { return (byte)KnownPacket.TileDamageUpdate; }
        }

        public Vec2I Position { get; set; }

        public TileLayer Layer { get; set; }

        public TileDamageStatus TileDamage { get; set; }


        public override void Read(IStarboundStream stream)
        {
            Position = Vec2I.FromStream(stream);
            Layer = (TileLayer)stream.ReadUInt8();
            TileDamage = TileDamageStatus.FromStream(stream);
        }

        public override void Write(IStarboundStream stream)
        {
            Position.WriteTo(stream);
            stream.WriteUInt8((byte)Layer);
            TileDamage.WriteTo(stream);
        }
    }
}