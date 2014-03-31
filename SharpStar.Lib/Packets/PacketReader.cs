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
        public readonly int MaxPacketLength = 1048576; // 1 MB (compressed, if applicable)
        public readonly int MaxInflatedPacketLength = 10485760; // 10 MB
        public readonly int NetworkBufferLength = 1024;

        private readonly Dictionary<byte, Func<IPacket>> _registeredPacketTypes;

        public byte[] NetworkBuffer { get; set; }

        private byte[] PacketBuffer = new byte[0];
        private long WorkingLength = long.MaxValue;
        private int DataIndex = 0;
        private bool Compressed = false;

        public PacketReader()
        {
            NetworkBuffer = new byte[5];

            _registeredPacketTypes = new Dictionary<byte, Func<IPacket>>();
        }

        public void RegisterPacketType(byte id, Type packetType)
        {
            if (!typeof(IPacket).IsAssignableFrom(packetType))
                throw new Exception("Type must be of IPacket!");

            _registeredPacketTypes.Add(id, Expression.Lambda<Func<IPacket>>(Expression.New(packetType)).Compile());
        }

        public List<IPacket> UpdateBuffer(int length)
        {

            if (length == 0)
                return new List<IPacket>();

            int index = PacketBuffer.Length;

            if (WorkingLength == long.MaxValue)
            {

                // We don't know the length of the packet yet, so keep going
                if (PacketBuffer.Length < index + length)
                    Array.Resize(ref PacketBuffer, index + length);

                Array.Copy(NetworkBuffer, 0, PacketBuffer, index, length);
                
                if (PacketBuffer.Length > 1)
                {
                    // Check to see if we have the entire length yet
                    int i;
                    for (i = 1; i < 5 && i < PacketBuffer.Length; i++)
                    {
                        if ((PacketBuffer[i] & 0x80) == 0)
                        {
                            MemoryStream ms = new MemoryStream(PacketBuffer);
                            ms.Seek(1, SeekOrigin.Begin);

                            StarboundStream stream = new StarboundStream(ms);

                            WorkingLength = stream.ReadSignedVLQ(out DataIndex);
                            DataIndex++;
                            Compressed = WorkingLength < 0;

                            stream.Close();
                            ms.Close();

                            if (Compressed)
                                WorkingLength = -WorkingLength;
                            if (WorkingLength > MaxPacketLength)
                                throw new IOException("Packet exceeded maximum permissible length.");

                            break;
                        }
                    }

                    if (i == 5)
                        throw new IOException("Packet exceeded maximum permissible length.");
                
                }
            }

            if (WorkingLength != long.MaxValue)
            {

                if (PacketBuffer.Length < index + length)
                    Array.Resize(ref PacketBuffer, index + length);

                Array.Copy(NetworkBuffer, 0, PacketBuffer, index, length);

                if (PacketBuffer.Length >= WorkingLength + DataIndex)
                {

                    // Ready to decode packet
                    var data = new byte[WorkingLength];
                    
                    Array.Copy(PacketBuffer, DataIndex, data, 0, WorkingLength);
                    
                    if (Compressed)
                        data = ZlibStream.UncompressBuffer(data);
                    
                    var packets = Decode(PacketBuffer[0], data);
                    
                    Array.Copy(PacketBuffer, DataIndex + WorkingLength, PacketBuffer, 0,
                        PacketBuffer.Length - (DataIndex + WorkingLength));
                    Array.Resize(ref PacketBuffer, (int)(PacketBuffer.Length - (DataIndex + WorkingLength)));
                    
                    WorkingLength = long.MaxValue;
                
                    return packets;
                
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