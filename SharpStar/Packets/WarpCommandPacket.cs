using System;
using SharpStar.DataTypes;
using SharpStar.Networking;

namespace SharpStar.Packets
{

    public class WarpCommandPacket : IPacket
    {
        public byte PacketId
        {
            get
            {
                return 10;
            }
        }

        public bool Ignore { get; set; }

        public WarpType WarpType { get; set; }

        public WorldCoordinate Coordinates { get; set; }

        public string Player { get; set; }

        public WarpCommandPacket()
        {
            Player = String.Empty;
            Coordinates = new WorldCoordinate();
        }

        public void Read(StarboundStream stream)
        {
            WarpType = (WarpType)stream.ReadUInt32();
            Coordinates = stream.ReadWorldCoordinate();
            Player = stream.ReadString();
        }

        public void Write(StarboundStream stream)
        {
            stream.WriteUInt32((uint)WarpType);
            stream.WriteWorldCoordinate(Coordinates);
            stream.WriteString(Player);
        }

    }

    public enum WarpType
    {
        MoveShip = 1,
        WarpUp = 2,
        WarpOtherShip = 3,
        WarpDown = 4,
        WarpHome = 5
    }

}
