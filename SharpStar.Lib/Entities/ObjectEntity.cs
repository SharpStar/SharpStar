using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Entities
{
    public class ObjectEntity : Entity
    {

        public string Object { get; set; }

        public Variant Information { get; set; }

        public byte[] Unknown { get; set; }

        public override void WriteTo(IStarboundStream stream)
        {
            stream.WriteUInt8((byte)EntityType);

            using (MemoryStream ms = new MemoryStream())
            {
                using (StarboundStream s = new StarboundStream(ms))
                {
                    s.WriteString(Object);
                    s.WriteVariant(Information);
                    s.WriteUInt8Array(Unknown, false);
                }

                stream.WriteUInt8Array(ms.ToArray());
            }
        
            stream.WriteSignedVLQ(EntityId);
        
        }

    }
}
