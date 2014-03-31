using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class UpdateWorldPropertiesPacket : Packet
    {
        public override byte PacketId
        {
            get { return 47; }
        }

        public byte NumPairs { get; set; }

        public string PropertyName { get; set; }

        public Variant PropertyValue { get; set; }

        public override void Read(IStarboundStream stream)
        {
            NumPairs = stream.ReadUInt8();
            PropertyName = stream.ReadString();
            PropertyValue = stream.ReadVariant();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt8(NumPairs);
            stream.WriteString(PropertyName);
            stream.WriteVariant(PropertyValue);
        }
    }
}