using System;
using System.IO;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Entities
{
    public class Entity
    {

        public EntityType EntityType { get; set; }

        public byte[] StoreData { get; set; }

        public long EntityId { get; set; }

        public Entity()
        {
            StoreData = new byte[0];
        }

        public static Entity FromStream(IStarboundStream stream)
        {

            int discarded;

            EntityType et = (EntityType)stream.ReadUInt8();

            byte[] storeData = stream.ReadUInt8Array();

            Entity ent;

            if (et == EntityType.Projectile)
            {

                ProjectileEntity pent = new ProjectileEntity();

                using (MemoryStream ms = new MemoryStream(storeData))
                {
                    using (StarboundStream s = new StarboundStream(ms))
                    {
                        pent.Projectile = s.ReadString();
                        pent.Information = s.ReadVariant().Value as VariantDict;
                    }
                }

                ent = pent;

            }
            else if (et == EntityType.Player)
            {

                PlayerEntity pent = new PlayerEntity();

                using (MemoryStream ms = new MemoryStream(storeData))
                {
                    using (StarboundStream s = new StarboundStream(ms))
                    {

                        bool uuid = s.ReadBoolean();

                        if (uuid)
                        {

                            byte[] uuidDat = s.ReadUInt8Array(16);

                            pent.UUID = BitConverter.ToString(uuidDat, 0).Replace("-", "").ToLower();
                        
                        }

                    }
                }

                ent = pent;

            }
            else
            {
                ent = new Entity();
            }

            ent.EntityType = et;
            ent.StoreData = storeData;
            ent.EntityId = stream.ReadSignedVLQ(out discarded);

            return ent;

        }

        public virtual void WriteTo(IStarboundStream stream)
        {
            stream.WriteUInt8((byte)EntityType);
            stream.WriteUInt8Array(StoreData);
            stream.WriteSignedVLQ(EntityId);
        }

    }
}
