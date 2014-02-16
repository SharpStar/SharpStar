using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Networking;
using SharpStar.DataTypes;

namespace SharpStar.Packets
{
    public class WorldStartPacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 12;
            }
            set
            {
            }
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

        public void Read(StarboundStream stream)
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

        public void Write(StarboundStream stream)
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
