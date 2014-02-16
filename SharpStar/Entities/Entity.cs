using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Misc;
using SharpStar.Networking;

namespace SharpStar.Entities
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

        public static Entity FromStream(StarboundStream stream)
        {

            int discarded;

            Entity ent = new Entity();
            ent.EntityType = (EntityType)stream.ReadUInt8();
            ent.StoreData = stream.ReadUInt8Array();
            ent.EntityId = stream.ReadSignedVLQ(out discarded);

            return ent;

        }

        public void WriteTo(StarboundStream stream)
        {
            stream.WriteUInt8((byte)EntityType);
            stream.WriteUInt8Array(StoreData);
            stream.WriteSignedVLQ(EntityId);
        }

    }
}
