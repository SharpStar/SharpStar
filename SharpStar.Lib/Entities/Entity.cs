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

            Entity ent = new Entity();
            ent.EntityType = (EntityType)stream.ReadUInt8();
            ent.StoreData = stream.ReadUInt8Array();
            ent.EntityId = stream.ReadSignedVLQ(out discarded);

            return ent;

        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteUInt8((byte)EntityType);
            stream.WriteUInt8Array(StoreData);
            stream.WriteSignedVLQ(EntityId);
        }

    }
}
