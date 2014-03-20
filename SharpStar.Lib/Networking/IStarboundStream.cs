using System.IO;
using SharpStar.Lib.DataTypes;

namespace SharpStar.Lib.Networking
{
    public interface IStarboundStream
    {

        Stream BaseStream { get; set; }

        long Length { get; }

        long Position { get; }

        int Read(byte[] buffer, int offset, int count);

        long Seek(long offset, SeekOrigin origin);

        void SetLength(long value);

        void Write(byte[] buffer, int offset, int count);

        byte ReadUInt8();

        void WriteUInt8(byte value);

        sbyte ReadInt8();

        void WriteInt8(sbyte value);

        ushort ReadUInt16();

        void WriteUInt16(ushort value);

        short ReadInt16();

        void WriteInt16(short value);

        uint ReadUInt32();

        void WriteUInt32(uint value);

        int ReadInt32();

        void WriteInt32(int value);

        ulong ReadUInt64();

        void WriteUInt64(ulong value);

        long ReadInt64();

        void WriteInt64(long value);

        byte[] ReadUInt8Array();

        byte[] ReadUInt8Array(int length);

        void WriteUInt8Array(byte[] value, bool includeLength = true);

        sbyte[] ReadInt8Array();

        sbyte[] ReadInt8Array(int length);

        void WriteInt8Array(sbyte[] value, bool includeLength);

        ushort[] ReadUInt16Array();

        ushort[] ReadUInt16Array(int length);

        void WriteUInt16Array(ushort[] value, bool includeLength = true);

        short[] ReadInt16Array();

        short[] ReadInt16Array(int length);

        void WriteInt16Array(short[] value, bool includeLength = true);

        uint[] ReadUInt32Array();

        uint[] ReadUInt32Array(int length);

        void WriteUInt32Array(uint[] value, bool includeLength = true);

        int[] ReadInt32Array();

        int[] ReadInt32Array(int length);

        void WriteInt32Array(int[] value, bool includeLength = true);

        ulong[] ReadUInt64Array();

        ulong[] ReadUInt64Array(int length);

        void WriteUInt64Array(ulong[] value, bool includeLength = true);

        long[] ReadInt64Array();

        long[] ReadInt64Array(int length);

        float ReadSingle();

        void WriteSingle(float value);

        double ReadDouble();

        void WriteDouble(double value);

        bool ReadBoolean();

        void WriteBoolean(bool value);

        void WriteVLQ(ulong value);

        void WriteSignedVLQ(long value);

        ulong ReadVLQ(out int length);

        long ReadSignedVLQ(out int length);

        string ReadString();

        void WriteString(string value);

        Variant ReadVariant();

        void WriteVariant(Variant variant);

        WorldCoordinate ReadWorldCoordinate();

        void WriteWorldCoordinate(WorldCoordinate coordinate);

    }
}
