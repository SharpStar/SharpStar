using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SharpStar.DataTypes;

namespace SharpStar.Networking
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet) and StarryPy (https://github.com/CarrotsAreMediocre/StarryPy)
    public class StarboundStream : Stream
    {
        public StarboundStream(Stream baseStream)
        {
            BaseStream = baseStream;
            StringEncoding = Encoding.UTF8;
        }

        public Encoding StringEncoding { get; set; }

        public Stream BaseStream { get; set; }

        public override bool CanRead { get { return BaseStream.CanRead; } }

        public override bool CanSeek { get { return BaseStream.CanSeek; } }

        public override bool CanWrite { get { return BaseStream.CanWrite; } }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override long Length
        {
            get { return BaseStream.Length; }
        }

        public override long Position
        {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        public byte ReadUInt8()
        {
            int value = BaseStream.ReadByte();
            if (value == -1)
                throw new EndOfStreamException();
            return (byte)value;
        }

        public void WriteUInt8(byte value)
        {
            WriteByte(value);
        }

        public sbyte ReadInt8()
        {
            return (sbyte)ReadUInt8();
        }

        public void WriteInt8(sbyte value)
        {
            WriteUInt8((byte)value);
        }

        public ushort ReadUInt16()
        {
            return (ushort)(
                (ReadUInt8() << 8) |
                ReadUInt8());
        }

        public void WriteUInt16(ushort value)
        {
            Write(new[]
                  {
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF)
            }, 0, 2);
        }

        public short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        public void WriteInt16(short value)
        {
            WriteUInt16((ushort)value);
        }

        public uint ReadUInt32()
        {
            return (uint)(
                (ReadUInt8() << 24) |
                (ReadUInt8() << 16) |
                (ReadUInt8() << 8) |
                ReadUInt8());
        }

        public void WriteUInt32(uint value)
        {
            Write(new[]
                  {
                (byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0xFF0000) >> 16),
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF)
            }, 0, 4);
        }

        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public void WriteInt32(int value)
        {
            WriteUInt32((uint)value);
        }

        public ulong ReadUInt64()
        {
            return unchecked(
                ((ulong)ReadUInt8() << 56) |
                ((ulong)ReadUInt8() << 48) |
                ((ulong)ReadUInt8() << 40) |
                ((ulong)ReadUInt8() << 32) |
                ((ulong)ReadUInt8() << 24) |
                ((ulong)ReadUInt8() << 16) |
                ((ulong)ReadUInt8() << 8) |
                (ulong)ReadUInt8());
        }

        public void WriteUInt64(ulong value)
        {
            Write(new[]
                  {
                (byte)((value & 0xFF00000000000000) >> 56),
                (byte)((value & 0xFF000000000000) >> 48),
                (byte)((value & 0xFF0000000000) >> 40),
                (byte)((value & 0xFF00000000) >> 32),
                (byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0xFF0000) >> 16),
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF)
            }, 0, 8);
        }

        public long ReadInt64()
        {
            return (long)ReadUInt64();
        }

        public void WriteInt64(long value)
        {
            WriteUInt64((ulong)value);
        }

        public byte[] ReadUInt8Array()
        {
            int discarded;
            int length = (int)ReadVLQ(out discarded);
            return ReadUInt8Array(length);
        }

        public byte[] ReadUInt8Array(int length)
        {
            var result = new byte[length];
            if (length == 0) return result;
            int n = length;
            while (true)
            {
                n -= Read(result, length - n, n);
                if (n == 0)
                    break;
                Thread.Sleep(1);
            }
            return result;
        }

        public void WriteUInt8Array(byte[] value, bool includeLength = true)
        {
            if (includeLength)
                WriteVLQ((ulong)value.Length);
            Write(value, 0, value.Length);
        }

        public sbyte[] ReadInt8Array()
        {
            return (sbyte[])(Array)ReadUInt8Array();
        }

        public sbyte[] ReadInt8Array(int length)
        {
            return (sbyte[])(Array)ReadUInt8Array(length);
        }

        public void WriteInt8Array(sbyte[] value, bool includeLength)
        {
            if (includeLength)
                WriteVLQ((ulong)value.Length);
            Write((byte[])(Array)value, 0, value.Length);
        }

        public ushort[] ReadUInt16Array()
        {
            int discarded;
            int length = (int)ReadVLQ(out discarded);
            return ReadUInt16Array(length);
        }

        public ushort[] ReadUInt16Array(int length)
        {
            var result = new ushort[length];
            if (length == 0) return result;
            for (int i = 0; i < length; i++)
                result[i] = ReadUInt16();
            return result;
        }

        public void WriteUInt16Array(ushort[] value, bool includeLength = true)
        {
            if (includeLength)
                WriteVLQ((ulong)value.Length);
            for (int i = 0; i < value.Length; i++)
                WriteUInt16(value[i]);
        }

        public short[] ReadInt16Array()
        {
            return (short[])(Array)ReadUInt16Array();
        }

        public short[] ReadInt16Array(int length)
        {
            return (short[])(Array)ReadUInt16Array(length);
        }

        public void WriteInt16Array(short[] value, bool includeLength = true)
        {
            WriteUInt16Array((ushort[])(Array)value, includeLength);
        }

        public uint[] ReadUInt32Array()
        {
            int discarded;
            int length = (int)ReadVLQ(out discarded);
            return ReadUInt32Array(length);
        }

        public uint[] ReadUInt32Array(int length)
        {
            var result = new uint[length];
            if (length == 0) return result;
            for (int i = 0; i < length; i++)
                result[i] = ReadUInt32();
            return result;
        }

        public void WriteUInt32Array(uint[] value, bool includeLength = true)
        {
            if (includeLength)
                WriteVLQ((ulong)value.Length);
            for (int i = 0; i < value.Length; i++)
                WriteUInt32(value[i]);
        }

        public int[] ReadInt32Array()
        {
            return (int[])(Array)ReadUInt32Array();
        }

        public int[] ReadInt32Array(int length)
        {
            return (int[])(Array)ReadUInt32Array(length);
        }

        public void WriteInt32Array(int[] value, bool includeLength = true)
        {
            WriteUInt32Array((uint[])(Array)value, includeLength);
        }

        public ulong[] ReadUInt64Array()
        {
            int discarded;
            int length = (int)ReadVLQ(out discarded);
            return ReadUInt64Array(length);
        }

        public ulong[] ReadUInt64Array(int length)
        {
            var result = new ulong[length];
            if (length == 0) return result;
            for (int i = 0; i < length; i++)
                result[i] = ReadUInt64();
            return result;
        }

        public void WriteUInt64Array(ulong[] value, bool includeLength = true)
        {
            if (includeLength)
                WriteVLQ((ulong)value.Length);
            for (int i = 0; i < value.Length; i++)
                WriteUInt64(value[i]);
        }

        public long[] ReadInt64Array()
        {
            return (long[])(Array)ReadUInt64Array();
        }

        public long[] ReadInt64Array(int length)
        {
            return (long[])(Array)ReadUInt64Array(length);
        }

        public void WriteInt64Array(long[] value, bool includeLength = true)
        {
            WriteUInt64Array((ulong[])(Array)value, includeLength);
        }

        public unsafe float ReadSingle()
        {
            uint value = ReadUInt32();
            return *(float*)&value;
        }

        public unsafe void WriteSingle(float value)
        {
            WriteUInt32(*(uint*)&value);
        }

        public unsafe double ReadDouble()
        {
            ulong value = ReadUInt64();
            return *(double*)&value;
        }

        public unsafe void WriteDouble(double value)
        {
            WriteUInt64(*(ulong*)&value);
        }

        public bool ReadBoolean()
        {
            return ReadUInt8() != 0;
        }

        public void WriteBoolean(bool value)
        {
            WriteUInt8(value ? (byte)1 : (byte)0);
        }

        public void WriteVLQ(ulong value)
        {
            var buffer = CreateVLQ(value);
            Write(buffer, 0, buffer.Length);
        }

        public void WriteSignedVLQ(long value)
        {
            var buffer = CreateSignedVLQ(value);
            Write(buffer, 0, buffer.Length);
        }

        public ulong ReadVLQ(out int length)
        {

            ulong value = 0L;

            length = 0;

            while (true)
            {

                byte tmp = ReadUInt8();

                value = (value << 7) | (ulong)(tmp & 0x7f);

                length++;

                if ((tmp & 0x80) == 0)
                    break;

            }

            return value;

        }

        public long ReadSignedVLQ(out int length)
        {

            ulong value = ReadVLQ(out length);

            if ((value & 1) == 0x00)
                return (long)value >> 1;

            return -((long)(value >> 1) + 1);

        }

        public string ReadString()
        {
            return StringEncoding.GetString(ReadUInt8Array());
        }

        public void WriteString(string value)
        {
            WriteUInt8Array(StringEncoding.GetBytes(value));
        }

        public Variant ReadVariant()
        {
            return Variant.FromStream(this);
        }

        public void WriteVariant(Variant variant)
        {
            variant.WriteTo(this);
        }

        public WorldCoordinate ReadWorldCoordinate()
        {
            return WorldCoordinate.FromStream(this);
        }

        public void WriteWorldCoordinate(WorldCoordinate coordinate)
        {
            coordinate.WriteTo(this);
        }

        public static byte[] CreateVLQ(ulong value)
        {

            var result = new List<byte>();

            if (value == 0)
                result.Add(0);

            while (value > 0)
            {

                byte tmp = (byte)(value & 0x7f);

                value >>= 7;

                if (value != 0)
                    tmp |= 0x80;

                result.Insert(0, tmp);

            }

            if (result.Count > 1)
            {
                result[0] |= 0x80;
                result[result.Count - 1] ^= 0x80;
            }

            return result.ToArray();

        }

        public static byte[] CreateSignedVLQ(long value)
        {

            long result = Math.Abs(value * 2);

            if (value < 0)
                result -= 1;

            return CreateVLQ((ulong)result);

        }

        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);

            if (disposing)
            {
                if (BaseStream != null)
                {
                    BaseStream.Close();
                    BaseStream.Dispose();
                }
            }

            BaseStream = null;

        }

    }
}
