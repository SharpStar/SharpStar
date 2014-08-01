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

namespace SharpStar.Lib.Packets
{
    public class WorldStartPacket : Packet
    {

        public override byte PacketId
        {
            get { return 14; }
        }

        public Variant Planet { get; set; }

        public Variant WorldStructure { get; set; }

        public byte[] Sky { get; set; }

        public byte[] Weather { get; set; }

        public float SpawnX { get; set; }

        public float SpawnY { get; set; }

        public Variant WorldProperties { get; set; }

        public uint ClientId { get; set; }

        public bool Local { get; set; }

        public override void Read(IStarboundStream stream)
        {
            Planet = stream.ReadVariant();
            WorldStructure = stream.ReadVariant();
            Sky = stream.ReadUInt8Array();
            Weather = stream.ReadUInt8Array();
            SpawnX = stream.ReadSingle();
            SpawnY = stream.ReadSingle();
            WorldProperties = stream.ReadVariant();
            ClientId = stream.ReadUInt32();
            Local = stream.ReadBoolean();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteVariant(Planet);
            stream.WriteVariant(WorldStructure);
            stream.WriteUInt8Array(Sky);
            stream.WriteUInt8Array(Weather);
            stream.WriteSingle(SpawnX);
            stream.WriteSingle(SpawnY);
            stream.WriteVariant(WorldProperties);
            stream.WriteUInt32(ClientId);
            stream.WriteBoolean(Local);
        }
    }
}