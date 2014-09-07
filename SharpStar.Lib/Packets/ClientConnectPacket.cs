// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet)
    public class ClientConnectPacket : Packet
    {
        public override byte PacketId
        {
            get { return (byte)KnownPacket.ClientConnect; }
        }

        public string AssetDigest;
        public Variant Claim;
        public byte[] UUID;
        public string PlayerName;
        public string Species;
        public byte[] Shipworld;
        public string Account;


        public override void Read(IStarboundStream stream)
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

        public override void Write(IStarboundStream stream)
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