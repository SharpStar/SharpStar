using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class WorldStartPacket : IPacket
    {
        public byte PacketId
        {
            get { return 14; }
        }

        public bool Ignore { get; set; }


        public Variant Planet { get; set; }

        public Variant WorldStructure { get; set; }

        public byte[] Sky { get; set; }

        public byte[] Weather { get; set; }

        public float SpawnX { get; set; }

        public float SpawnY { get; set; }

        public Variant WorldProperties { get; set; }

        public uint ClientId { get; set; }

        public bool Local { get; set; }

        public WorldStartPacket()
        {
        }

        public void Read(IStarboundStream stream)
        {
            Planet = stream.ReadVariant();
            WorldStructure = stream.ReadVariant();
            Sky = stream.ReadUInt8Array();
            Weather = stream.ReadUInt8Array();
            SpawnX = stream.ReadSingle();
            SpawnY = stream.ReadSingle();
            WorldProperties = stream.ReadVariant();
            ClientId = stream.ReadUInt32();
            Local = stream.ReadBoolean();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteVariant(Planet);
            stream.WriteVariant(WorldStructure);
            stream.WriteUInt8Array(Sky);
            stream.WriteUInt8Array(Weather);
            stream.WriteSingle(SpawnX);
            stream.WriteSingle(SpawnY);
            stream.WriteVariant(WorldProperties);
            stream.WriteUInt32(ClientId);
            stream.WriteBoolean(Local);
        }
    }
}