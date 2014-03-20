using System.Collections.Generic;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class EntityCreatePacket : IPacket
    {
        public byte PacketId
        {
            get { return 42; }
        }

        public bool Ignore { get; set; }

        public List<Entity> Entities { get; set; }

        public EntityCreatePacket()
        {
            Entities = new List<Entity>();
        }

        public void Read(IStarboundStream stream)
        {
            while ((stream.Length - stream.Position) > 0)
            {
                Entities.Add(Entity.FromStream(stream));
            }
        }

        public void Write(IStarboundStream stream)
        {
            foreach (var entity in Entities)
            {
                entity.WriteTo(stream);
            }
        }
    }
}