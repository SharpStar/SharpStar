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
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class GiveItemPacket : Packet
    {
        public override byte PacketId
        {
            get { return (byte)KnownPacket.GiveItem; }
        }

        public string ItemName { get; set; }

        public ulong Count { get; set; }

        public VariantDict ItemProperties { get; set; }

        public GiveItemPacket()
        {
            ItemName = String.Empty;
            ItemProperties = new VariantDict();
        }

        public override void Read(IStarboundStream stream)
        {
            int discarded;

            ItemName = stream.ReadString();
            Count = stream.ReadVLQ(out discarded);
            ItemProperties = (VariantDict) stream.ReadVariant().Value;
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteString(ItemName);
            stream.WriteVLQ(Count);
            stream.WriteVariant(new Variant(ItemProperties));
        }
    }
}