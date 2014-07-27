using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using Ionic.Zlib;
using SharpStar.Lib.Networking;

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

            RegisterPacketType(0, typeof(ProtocolVersionPacket));
            RegisterPacketType(1, typeof(ConnectionResponsePacket));
            RegisterPacketType(2, typeof(DisconnectResponsePacket));
            RegisterPacketType(3, typeof(HandshakeChallengePacket));
            RegisterPacketType(4, typeof(ChatReceivedPacket));
            RegisterPacketType(5, typeof(UniverseTimeUpdatePacket));
            RegisterPacketType(7, typeof(ClientConnectPacket));
            RegisterPacketType(8, typeof(ClientDisconnectPacket));
            RegisterPacketType(9, typeof(HandshakeResponsePacket));
            RegisterPacketType(10, typeof(WarpCommandPacket));
            RegisterPacketType(11, typeof(ChatSentPacket));
            RegisterPacketType(13, typeof(ClientContextUpdatePacket));
            RegisterPacketType(14, typeof(WorldStartPacket));
            RegisterPacketType(15, typeof(WorldStopPacket));
            RegisterPacketType(19, typeof(TileDamageUpdatePacket));
            RegisterPacketType(21, typeof(GiveItemPacket));
            RegisterPacketType(23, typeof(EnvironmentUpdatePacket));
            RegisterPacketType(24, typeof(EntityInteractResultPacket));
            RegisterPacketType(28, typeof(RequestDropPacket));
            RegisterPacketType(33, typeof(OpenContainerPacket));
            RegisterPacketType(34, typeof(CloseContainerPacket));
            RegisterPacketType(42, typeof(EntityCreatePacket));
            RegisterPacketType(43, typeof(EntityUpdatePacket));
            RegisterPacketType(44, typeof(EntityDestroyPacket));
            RegisterPacketType(45, typeof(DamageNotificationPacket));
            RegisterPacketType(47, typeof(UpdateWorldPropertiesPacket));
            RegisterPacketType(48, typeof(HeartbeatPacket));
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

                        WorkingLength = s.ReadSignedVLQ(out DataIndex);
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

                packet.Read(stream);

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