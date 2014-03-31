using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class EntityInteractResultPacket : Packet
    {
        public override byte PacketId
        {
            get { return 24; }
        }

        public uint ClientId { get; set; }

        public int EntityId { get; set; }

        public Variant Results { get; set; }

        public override void Read(IStarboundStream stream)
        {
            ClientId = stream.ReadUInt32();
            EntityId = stream.ReadInt32();
            Results = stream.ReadVariant();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt32(ClientId);
            stream.WriteInt32(EntityId);
            stream.WriteVariant(Results);
        }
    }
}