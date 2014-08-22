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
using System.IO;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Entities
{
    public class Entity : IWriteable
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
            EntityType et = (EntityType)stream.ReadUInt8();

            byte[] storeData = stream.ReadUInt8Array();

            Entity ent;

            if (et == EntityType.Projectile)
            {

                ProjectileEntity pent = new ProjectileEntity();

                using (StarboundStream s = new StarboundStream(storeData))
                {
                    pent.Projectile = s.ReadString();
                    pent.Information = s.ReadVariant().Value as VariantDict;
                    pent.Unknown1 = s.ReadUInt8Array(17);
                    pent.ThrowerEntityId = s.ReadSignedVLQ();
                    pent.Unknown2 = s.ReadUInt8Array((int)(s.Length - s.Position));
                }

                ent = pent;

            }
            else if (et == EntityType.Player)
            {

                PlayerEntity pent = new PlayerEntity();

                using (StarboundStream s = new StarboundStream(storeData))
                {

                    bool uuid = s.ReadBoolean();

                    if (uuid)
                    {

                        byte[] uuidDat = s.ReadUInt8Array(16);

                        pent.UUID = BitConverter.ToString(uuidDat, 0).Replace("-", "").ToLower();

                    }

                }

                ent = pent;

            }
            else if (et == EntityType.Object)
            {

                ObjectEntity oent = new ObjectEntity();

                using (StarboundStream s = new StarboundStream(storeData))
                {
                    oent.Object = s.ReadString();
                    oent.Information = s.ReadVariant();
                    oent.Unknown = s.ReadUInt8Array((int)(s.Length - s.Position));
                }

                ent = oent;

            }
            else
            {
                ent = new Entity();
            }

            ent.EntityType = et;
            ent.StoreData = storeData;
            ent.EntityId = stream.ReadSignedVLQ();

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
