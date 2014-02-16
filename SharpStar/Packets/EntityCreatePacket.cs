using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Entities;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class EntityCreatePacket : ServerPacket
    {

        public override byte PacketId
        {
            get
            {
                return 40;
            }
            set
            {
            }
        }

        public override bool Ignore { get; set; }

        public Entity[] Entities { get; set; }

        public EntityCreatePacket()
        {
            Entities = new Entity[0];
        }

        public override void Read(StarboundStream stream)
        {

            var entities = new List<Entity>();

            while ((stream.Length - stream.Position) > 0)
            {
                entities.Add(Entity.FromStream(stream));
            }

            Entities = entities.ToArray();

        }

        public override void Write(StarboundStream stream)
        {
            foreach (var entity in Entities)
            {
                entity.WriteTo(stream);
            }
        }
    }
}
