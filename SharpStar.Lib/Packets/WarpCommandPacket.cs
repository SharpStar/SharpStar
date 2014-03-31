using System;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public class WarpCommandPacket : Packet
    {
        public override byte PacketId
        {
            get { return 10; }
        }

        public WarpType WarpType { get; set; }

        public WorldCoordinate Coordinates { get; set; }

        public string Player { get; set; }

        public WarpCommandPacket()
        {
            Player = String.Empty;
            Coordinates = new WorldCoordinate();
        }

        public override void Read(IStarboundStream stream)
        {
            WarpType = (WarpType) stream.ReadUInt32();
            Coordinates = stream.ReadWorldCoordinate();
            Player = stream.ReadString();
        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt32((uint) WarpType);
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