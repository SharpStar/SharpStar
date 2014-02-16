using System;
using SharpStar.DataTypes;
using SharpStar.Networking;

namespace SharpStar.Packets
{

    public class WarpCommandPacket : ClientPacket
    {
        public override byte PacketId
        {
            get
            {
                return 9;
            }
            set
            {
            }
        }
        public override bool Ignore { get; set; }

        public WarpType WarpType { get; set; }

        public WorldCoordinate Coordinates { get; set; }

        public string Player { get; set; }

        public WarpCommandPacket()
        {
            Player = String.Empty;
            Coordinates = new WorldCoordinate();
        }

        public override void Read(StarboundStream stream)
        {
            WarpType = (WarpType)stream.ReadUInt32();
            Coordinates = stream.ReadWorldCoordinate();
            Player = stream.ReadString();
        }

        public override void Write(StarboundStream stream)
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
