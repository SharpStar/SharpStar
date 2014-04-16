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

        private readonly Dictionary<byte, Func<IPacket>> _registeredPacketTypes;

        public byte[] NetworkBuffer { get; set; }

        private byte[] PacketBuffer = new byte[0];
        private long WorkingLength = long.MaxValue;
        private int DataIndex = 0;
        private bool Compressed = false;

        private byte _packetId;

        public PacketReader()
        {

            _registeredPacketTypes = new Dictionary<byte, Func<IPacket>>();

            NetworkBuffer = new byte[1024];

        }

        public void RegisterPacketType(byte id, Type packetType)
        {
            if (!typeof(IPacket).IsAssignableFrom(packetType))
                throw new Exception("Type must be of IPacket!");

            _registeredPacketTypes.Add(id, Expression.Lambda<Func<IPacket>>(Expression.New(packetType)).Compile());
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


                    if (WorkingLength == long.MaxValue)
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

                            packets.AddRange(Decode(_packetId, data));

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

        public List<IPacket> Decode(byte packetId, byte[] payload)
        {
            var memoryStream = new MemoryStream(payload);
            var stream = new StarboundStream(memoryStream);
            var packets = new List<IPacket>();

            IPacket packet;

            if (_registeredPacketTypes.ContainsKey(packetId))
            {
                packet = _registeredPacketTypes[packetId]();
            }
            else
            {
                packet = new UnknownPacket(Compressed, payload.Length, packetId);
            }

            if (packet != null)
            {

                packet.Read(stream);

                packets.Add(packet);

            }

            stream.Close();
            stream.Dispose();

            memoryStream.Close();
            memoryStream.Dispose();

            return packets;
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