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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using Ionic.Zlib;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Networking;
using SharpStar.Lib.Extensions;

namespace SharpStar.Lib.Packets
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet)
    public class PacketReader : IDisposable
    {
        public const int MaxPacketLength = 1048576; // 1 MB (compressed, if applicable)
        public const int MaxInflatedPacketLength = 10485760; // 10 MB
        public const int NetworkBufferLength = 1024;

        public static Dictionary<byte, Func<IPacket>> RegisteredPacketTypes;

        public byte[] NetworkBuffer { get; set; }

        private byte[] PacketBuffer = new byte[0];
        private long WorkingLength = long.MaxValue;
        private int DataIndex = 0;
        private bool Compressed = false;

        private byte _packetId;

        public PacketReader()
        {
            NetworkBuffer = new byte[1024];
        }

        static PacketReader()
        {
            RegisteredPacketTypes = new Dictionary<byte, Func<IPacket>>();

            RegisterPacketType((byte)KnownPacket.ProtocolVersion, typeof(ProtocolVersionPacket));
            RegisterPacketType((byte)KnownPacket.ConnectionResponse, typeof(ConnectionResponsePacket));
            RegisterPacketType((byte)KnownPacket.DisconnectResponse, typeof(DisconnectResponsePacket));
            RegisterPacketType((byte)KnownPacket.HandshakeChallenge, typeof(HandshakeChallengePacket));
            RegisterPacketType((byte)KnownPacket.ChatReceived, typeof(ChatReceivedPacket));
            RegisterPacketType((byte)KnownPacket.UniverseTimeUpdate, typeof(UniverseTimeUpdatePacket));
            RegisterPacketType((byte)KnownPacket.ClientConnect, typeof(ClientConnectPacket));
            RegisterPacketType((byte)KnownPacket.ClientDisconnect, typeof(ClientDisconnectPacket));
            RegisterPacketType((byte)KnownPacket.HandshakeResponse, typeof(HandshakeResponsePacket));
            RegisterPacketType((byte)KnownPacket.WarpCommand, typeof(WarpCommandPacket));
            RegisterPacketType((byte)KnownPacket.ChatSent, typeof(ChatSentPacket));
            RegisterPacketType((byte)KnownPacket.ClientContextUpdate, typeof(ClientContextUpdatePacket));
            RegisterPacketType((byte)KnownPacket.WorldStart, typeof(WorldStartPacket));
            RegisterPacketType((byte)KnownPacket.WorldStop, typeof(WorldStopPacket));
            //RegisterPacketType((byte)KnownPacket.TileDamageUpdate, typeof(TileDamageUpdatePacket));
            RegisterPacketType((byte)KnownPacket.GiveItem, typeof(GiveItemPacket));
            RegisterPacketType((byte)KnownPacket.EnvironmentUpdate, typeof(EnvironmentUpdatePacket));
            RegisterPacketType((byte)KnownPacket.EntityInteractResult, typeof(EntityInteractResultPacket));
            RegisterPacketType((byte)KnownPacket.DamageTileGroup, typeof(DamageTileGroupPacket));
            RegisterPacketType((byte)KnownPacket.RequestDrop, typeof(RequestDropPacket));
            RegisterPacketType((byte)KnownPacket.OpenContainer, typeof(OpenContainerPacket));
            RegisterPacketType((byte)KnownPacket.CloseContainer, typeof(CloseContainerPacket));
            RegisterPacketType((byte)KnownPacket.EntityCreate, typeof(EntityCreatePacket));
            RegisterPacketType((byte)KnownPacket.EntityUpdate, typeof(EntityUpdatePacket));
            RegisterPacketType((byte)KnownPacket.EntityDestroy, typeof(EntityDestroyPacket));
            RegisterPacketType((byte)KnownPacket.DamageNotification, typeof(DamageNotificationPacket));
            RegisterPacketType((byte)KnownPacket.UpdateWorldProperties, typeof(UpdateWorldPropertiesPacket));
            RegisterPacketType((byte)KnownPacket.Heartbeat, typeof(HeartbeatPacket));
            //RegisterPacketType((byte)KnownPacket.SpawnEntity, typeof(SpawnEntityPacket));
        }

        public static void RegisterPacketType(byte id, Type packetType)
        {
            if (!typeof(IPacket).IsAssignableFrom(packetType))
                throw new Exception("Type must be of IPacket!");

            RegisteredPacketTypes.Add(id, Expression.Lambda<Func<IPacket>>(Expression.New(packetType)).Compile());
        }

        public List<IPacket> UpdateBuffer(byte[] buf, int length)
        {

            int index = PacketBuffer.Length;

            if (buf == null)
            {
                if (PacketBuffer.Length < index + length)
                {
                    Array.Resize(ref PacketBuffer, index + length);
                }

                Buffer.BlockCopy(NetworkBuffer, 0, PacketBuffer, index, length);
            }

            using (MemoryStream ms = new MemoryStream(PacketBuffer))
            {
                using (StarboundStream s = new StarboundStream(ms))
                {

                    if (WorkingLength == long.MaxValue && s.Length > 1)
                    {

                        _packetId = s.ReadUInt8();

                        try
                        {

                            WorkingLength = s.ReadSignedVLQ(out DataIndex);
                        }
                        catch (Exception)
                        {
                            return new List<IPacket>();
                        }
                         
                        DataIndex++;

                        Compressed = WorkingLength < 0;

                        if (Compressed)
                            WorkingLength = -WorkingLength;

                    }

                    if (WorkingLength != long.MaxValue)
                    {
                        if (PacketBuffer.Length >= WorkingLength + DataIndex)
                        {

                            byte[] data = new byte[WorkingLength];

                            Buffer.BlockCopy(PacketBuffer, DataIndex, data, 0, (int)WorkingLength);

                            Buffer.BlockCopy(PacketBuffer, DataIndex + (int)WorkingLength, PacketBuffer, 0, PacketBuffer.Length - (DataIndex + (int)WorkingLength));
                            Array.Resize(ref PacketBuffer, (int)(PacketBuffer.Length - (DataIndex + WorkingLength)));

                            if (Compressed)
                                data = ZlibStream.UncompressBuffer(data);

                            var packets = new List<IPacket>();

                            packets.Add(Decode(_packetId, data));

                            WorkingLength = long.MaxValue;

                            if (PacketBuffer.Length > 0)
                            {
                                packets.AddRange(UpdateBuffer(PacketBuffer, PacketBuffer.Length));
                            }

                            return packets;
                        }

                    }

                }
            }

            return new List<IPacket>();

        }

        public IPacket Decode(byte packetId, byte[] payload)
        {
            var memoryStream = new MemoryStream(payload);
            var stream = new StarboundStream(memoryStream);

            IPacket packet;

            if (RegisteredPacketTypes.ContainsKey(packetId))
            {
                packet = RegisteredPacketTypes[packetId]();
            }
            else
            {
                packet = new UnknownPacket(Compressed, payload.Length, packetId);
            }

            if (packet != null)
            {

                packet.IsReceive = true;

                try
                {
                    packet.Read(stream);
                }
                catch (Exception e)
                {
                    SharpStarLogger.DefaultLogger.Error("Packet {0} caused and error!", packet.GetType().Name);

                    e.LogError();
                }

            }

            stream.Close();
            stream.Dispose();

            memoryStream.Close();
            memoryStream.Dispose();

            return packet;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            NetworkBuffer = null;
            PacketBuffer = null;
        }
    }
}