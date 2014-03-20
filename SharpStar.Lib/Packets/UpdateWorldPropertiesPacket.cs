using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class UpdateWorldPropertiesPacket : IPacket
    {
        public byte PacketId
        {
            get { return 47; }
        }

        public bool Ignore { get; set; }

        public byte NumPairs { get; set; }

        public string PropertyName { get; set; }

        public Variant PropertyValue { get; set; }

        public void Read(IStarboundStream stream)
        {
            NumPairs = stream.ReadUInt8();
            PropertyName = stream.ReadString();
            PropertyValue = stream.ReadVariant();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteUInt8(NumPairs);
            stream.WriteString(PropertyName);
            stream.WriteVariant(PropertyValue);
        }
    }
}