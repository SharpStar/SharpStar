using System.Collections.Generic;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class EntityCreatePacket : Packet
    {
        public override byte PacketId
        {
            get { return 42; }
        }

        public List<Entity> Entities { get; set; }

        public EntityCreatePacket()
        {
            Entities = new List<Entity>();
        }

        public override void Read(IStarboundStream stream)
        {
            while ((stream.Length - stream.Position) > 0)
            {
                Entities.Add(Entity.FromStream(stream));
            }
        }

        public override void Write(IStarboundStream stream)
        {
            foreach (var entity in Entities)
            {
                entity.WriteTo(stream);
            }
        }
    }
}