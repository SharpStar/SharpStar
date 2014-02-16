using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Entities;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class EntityCreatePacket : IPacket
    {

        public byte PacketId
        {
            get
            {
                return 40;
            }
        }

        public bool Ignore { get; set; }

        public List<Entity> Entities { get; set; }

        public EntityCreatePacket()
        {
            Entities = new List<Entity>();
        }

        public void Read(StarboundStream stream)
        {
            while ((stream.Length - stream.Position) > 0)
            {
                Entities.Add(Entity.FromStream(stream));
            }
        }

        public void Write(StarboundStream stream)
        {
            foreach (var entity in Entities)
            {
                entity.WriteTo(stream);
            }
        }
    }
}
