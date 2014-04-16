using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class SpawnEntityPacket : Packet
    {

        public override byte PacketId
        {
            get
            {
                return 29;
            }
        }

        public List<Entity> Entities { get; set; }

        public SpawnEntityPacket()
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
