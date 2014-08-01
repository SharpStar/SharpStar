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
using System.IO;
using System.Linq;
using System.Text;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Entities
{
    public class ObjectEntity : Entity
    {

        public string Object { get; set; }

        public Variant Information { get; set; }

        public byte[] Unknown { get; set; }

        public override void WriteTo(IStarboundStream stream)
        {
            stream.WriteUInt8((byte)EntityType);

            using (MemoryStream ms = new MemoryStream())
            {
                using (StarboundStream s = new StarboundStream(ms))
                {
                    s.WriteString(Object);
                    s.WriteVariant(Information);
                    s.WriteUInt8Array(Unknown, false);
                }

                stream.WriteUInt8Array(ms.ToArray());
            }
        
            stream.WriteSignedVLQ(EntityId);
        
        }

    }
}
