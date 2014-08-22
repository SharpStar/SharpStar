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

namespace SharpStar.Lib.Packets
{
    public class EntityDestroyPacket : Packet
    {
        public override byte PacketId
        {
            get { return (byte)KnownPacket.EntityDestroy; }
        }

        public long EntityId { get; set; }

        public bool Death { get; set; }

        public byte[] Unknown { get; set; }

        public EntityDestroyPacket()
        {
            Unknown = new byte[0];
        }

        public override void Read(IStarboundStream stream)
        {
            EntityId = stream.ReadSignedVLQ();
            Death = stream.ReadBoolean();

            Unknown = new byte[stream.Length - stream.Position];

            stream.Read(Unknown, 0, (int)(stream.Length - stream.Position));

        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteBoolean(Death);
            stream.Write(Unknown, 0, Unknown.Length);
        }
    }
}