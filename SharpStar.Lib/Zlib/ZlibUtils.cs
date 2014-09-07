using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpStar.Lib.Zlib
{
    public static class ZlibUtils
    {

        public static byte[] Compress(byte[] buffer)
        {
            return CompressAsync(buffer).Result;
        }

        public static async Task<byte[]> CompressAsync(byte[] buffer)
        {
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                using (MemoryStream ms = new MemoryStream())
                {

                    byte[] header = { 0x78, 0x9C };

                    await ms.WriteAsync(header, 0, header.Length);

                    using (var ds = new DeflateStream(ms, CompressionMode.Compress, true))
                    {
                        await stream.CopyToAsync(ds);
                    }

                    uint adler32 = Adler.Adler32(1, buffer, 0, buffer.Length);

                    byte[] checksum = new byte[4];
                    checksum[0] = (byte)((adler32 & 0xFF000000) >> 24);
                    checksum[1] = (byte)((adler32 & 0x00FF0000) >> 16);
                    checksum[2] = (byte)((adler32 & 0x0000FF00) >> 8);
                    checksum[3] = (byte)(adler32 & 0x000000FF);

                    await ms.WriteAsync(checksum, 0, checksum.Length);

                    return ms.ToArray();
                }
            }
        }

        public static byte[] Decompress(byte[] buffer)
        {
            return DecompressAsync(buffer).Result;
        }

        public static async Task<byte[]> DecompressAsync(byte[] buffer)
        {
            using (MemoryStream bms = new MemoryStream(buffer))
            {
                bms.SetLength(bms.Length - 4); //truncate last 4 bytes (adler32 checksum)

                //don't care about the header
                byte[] header = new byte[2];
                await bms.ReadAsync(header, 0, header.Length);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (var ds = new DeflateStream(bms, CompressionMode.Decompress))
                    {
                        await ds.CopyToAsync(ms);
                    }

                    return ms.ToArray();
                }
            }
        }

    }
}
