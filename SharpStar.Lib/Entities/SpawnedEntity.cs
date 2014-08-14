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

            using (MemoryStream ms = new MemoryStream(storeData))
            {
                using (StarboundStream ss = new StarboundStream(ms))
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
