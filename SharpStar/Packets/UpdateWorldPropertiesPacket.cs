using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.DataTypes;
using SharpStar.Networking;

namespace SharpStar.Packets
{
    public class UpdateWorldPropertiesPacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 47;
            }
        }

        public bool Ignore { get; set; }

        public byte NumPairs { get; set; }

        public string PropertyName { get; set; }

        public Variant PropertyValue { get; set; }

        public void Read(StarboundStream stream)
        {
            NumPairs = stream.ReadUInt8();
            PropertyName = stream.ReadString();
            PropertyValue = stream.ReadVariant();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteUInt8(NumPairs);
            stream.WriteString(PropertyName);
            stream.WriteVariant(PropertyValue);
        }
    }
}
