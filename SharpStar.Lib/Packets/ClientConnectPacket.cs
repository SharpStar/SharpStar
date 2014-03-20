using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet)
    public class ClientConnectPacket : IPacket
    {
        public byte PacketId
        {
            get { return 7; }
        }

        public bool Ignore { get; set; }

        public string AssetDigest;
        public Variant Claim;
        public byte[] UUID;
        public string PlayerName;
        public string Species;
        public byte[] Shipworld;
        public string Account;

        public ClientConnectPacket()
        {
        }

        public ClientConnectPacket(string assetDigest, Variant claim, byte[] uuid, string playerName,
            string species, byte[] shipworld, string account)
        {
            AssetDigest = assetDigest;
            Claim = claim;
            UUID = uuid;
            PlayerName = playerName;
            Species = species;
            Shipworld = shipworld;
            Account = account;
            Ignore = false;
        }


        public void Read(IStarboundStream stream)
        {
            AssetDigest = stream.ReadString();
            Claim = stream.ReadVariant();
            bool uuid = stream.ReadBoolean();
            if (uuid)
                UUID = stream.ReadUInt8Array(16);
            PlayerName = stream.ReadString();
            Species = stream.ReadString();
            Shipworld = stream.ReadUInt8Array();
            Account = stream.ReadString();
        }

        public void Write(IStarboundStream stream)
        {
            stream.WriteString(AssetDigest);
            stream.WriteVariant(Claim);
            stream.WriteBoolean(UUID != null);
            if (UUID != null)
                stream.WriteUInt8Array(UUID, false);
            stream.WriteString(PlayerName);
            stream.WriteString(Species);
            stream.WriteUInt8Array(Shipworld);
            stream.WriteString(Account);
        }
    }
}