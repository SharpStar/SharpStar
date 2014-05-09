using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Entities
{
    public class ProjectileEntity : Entity
    {

        public string Projectile { get; set; }

        public VariantDict Information { get; set; }

        public byte[] Unknown1 { get; set; }

        public byte[] Unknown2 { get; set; }

        public long ThrowerEntityId { get; set; }

        public override void WriteTo(IStarboundStream stream)
        {
            stream.WriteUInt8((byte)EntityType);

            using (MemoryStream ms = new MemoryStream())
            {

                using (StarboundStream s = new StarboundStream(ms))
                {
                    s.WriteString(Projectile);
                    s.WriteVariant(new Variant(Information));
                    s.WriteUInt8Array(Unknown1, false);
                    s.WriteSignedVLQ(ThrowerEntityId);
                    s.WriteUInt8Array(Unknown2, false);
                }

                stream.WriteUInt8Array(ms.ToArray());
            
            }
            
            stream.WriteSignedVLQ(EntityId);
        }

    }
}
