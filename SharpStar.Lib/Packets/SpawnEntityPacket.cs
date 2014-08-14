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
using SharpStar.Lib.Entities;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class SpawnEntityPacket : Packet
    {

        public override byte PacketId
        {
            get
            {
                return (byte)KnownPacket.SpawnEntity;
            }
        }

        public List<SpawnedEntity> SpawnedEntities { get; set; }

        public SpawnEntityPacket()
        {
            SpawnedEntities = new List<SpawnedEntity>();
        }


        public override void Read(IStarboundStream stream)
        {
            while ((stream.Length - stream.Position) > 0)
            {
                SpawnedEntities.Add(SpawnedEntity.FromStream(stream));
            }
        }

        public override void Write(IStarboundStream stream)
        {
            foreach (SpawnedEntity se in SpawnedEntities)
            {
                se.WriteTo(stream);
            }
        }

    }
}
