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
using System.IO;
using System.Linq;
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

        public static Dictionary<byte, Func<IPacket>> RegisteredPacketTypes;

        public ArraySegment<byte> NetworkBuffer { get; set; }
        private List<byte> PacketBuffer = new List<byte>();

        private long WorkingLength = long.MaxValue;
        private int DataIndex;
        private bool Compressed;

        private byte _packetId;

        public PacketReader()
        {
            NetworkBuffer = new ArraySegment<byte>();
            Compressed = false;
            DataIndex = 0;
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
            RegisterPacketType((byte)KnownPacket.SpawnEntity, typeof(SpawnEntityPacket));
        }

        public static void RegisterPacketType(byte id, Type packetType)
        {
            if (!typeof(IPacket).IsAssignableFrom(packetType))
                throw new Exception("Type must be of IPacket!");

            RegisteredPacketTypes.Add(id, Expression.Lambda<Func<IPacket>>(Expression.New(packetType)).Compile());
        }

        public IEnumerable<IPacket> UpdateBuffer(bool shouldCopy)
        {
            if (shouldCopy)
            {
                PacketBuffer.AddRange(NetworkBuffer);
            }

            Queue<byte[]> toProcess = new Queue<byte[]>();
            toProcess.Enqueue(PacketBuffer.ToArray());

            while (toProcess.Count > 0)
            {

                byte[] arr = toProcess.Dequeue();

                using (StarboundStream s = new StarboundStream(arr))
                {
                    if (WorkingLength == long.MaxValue && s.Length > 1)
                    {
                        _packetId = s.ReadUInt8();

                        try
                        {
                            WorkingLength = s.ReadSignedVLQ();
                        }
                        catch
                        {
                            WorkingLength = long.MaxValue;

                            yield break;
                        }

                        DataIndex = (int)s.Position;

                        Compressed = WorkingLength < 0;

                        if (Compressed)
                            WorkingLength = -WorkingLength;
                    }

                    if (WorkingLength != long.MaxValue)
                    {

                        if (s.Length >= WorkingLength + DataIndex)
                        {

                            if (s.Position != DataIndex)
                                s.Seek(DataIndex, SeekOrigin.Begin);

                            byte[] data = s.ReadUInt8Array((int)WorkingLength);

                            if (Compressed)
                            {
                                data = ZlibStream.UncompressBuffer(data);
                            }

                            IPacket packet = Decode(_packetId, data);

                            WorkingLength = long.MaxValue;

                            byte[] rest = s.ReadToEnd();
                            PacketBuffer = rest.ToList();

                            if (rest.Length > 0)
                                toProcess.Enqueue(rest);

                            yield return packet;
                        }

                    }

                }

            }

        }

        public IPacket Decode(byte packetId, byte[] payload)
        {
            StarboundStream stream = new StarboundStream(payload);

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

            stream.Dispose();

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
                PacketBuffer.Clear();
            }

            PacketBuffer = null;
        }
    }
}