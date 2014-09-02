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
using SharpStar.Lib.Misc;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Entities
{
    public class SpawnedEntity : IWriteable
    {

        public EntityType EntityType { get; set; }

        public byte[] StoreData { get; set; }

        public static SpawnedEntity FromStream(IStarboundStream stream)
        {
            SpawnedEntity se;
            EntityType et = (EntityType)stream.ReadUInt8();
            byte[] storeData = stream.ReadUInt8Array((int)(stream.Length - stream.Position));


            using (StarboundStream ss = new StarboundStream(storeData))
            {
                if (et == EntityType.Projectile)
                {
                    var sp = new SpawnedProjectile();
                    sp.ProjectileKey = ss.ReadString();
                    sp.Parameters = ss.ReadVariant();

                    se = sp;
                }
                else
                {
                    se = new SpawnedEntity();
                }
            }

            se.EntityType = et;
            se.StoreData = storeData;

            return se;
        }

        public virtual void WriteTo(IStarboundStream stream)
        {
            stream.WriteUInt8((byte)EntityType);
            stream.WriteUInt8Array(StoreData, false);
        }
    }
}
