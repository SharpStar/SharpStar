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
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpStar.Lib.Extensions;

namespace SharpStar.Lib.Zlib
{
    public static class ZlibUtils
    {

        public static byte[] Compress(byte[] buffer)
        {
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {

                        byte[] header = { 0x78, 0x9C };

                        ms.Write(header, 0, header.Length);

                        using (var ds = new DeflateStream(ms, CompressionMode.Compress, true))
                        {
                            stream.CopyTo(ds);
                        }

                        uint adler32 = Adler.Adler32(1, buffer, 0, buffer.Length);

                        byte[] checksum = new byte[4];
                        checksum[0] = (byte)((adler32 & 0xFF000000) >> 24);
                        checksum[1] = (byte)((adler32 & 0x00FF0000) >> 16);
                        checksum[2] = (byte)((adler32 & 0x0000FF00) >> 8);
                        checksum[3] = (byte)(adler32 & 0x000000FF);

                        ms.Write(checksum, 0, checksum.Length);
                    }
                    catch
                    {
                    }

                    return ms.ToArray();
                }
            }
        }

        public static byte[] Decompress(byte[] buffer)
        {
            using (MemoryStream bms = new MemoryStream(buffer))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        bms.SetLength(bms.Length - 4); //truncate last 4 bytes (adler32 checksum)

                        //don't care about the header
                        bms.Seek(2, SeekOrigin.Begin);

                        using (var ds = new DeflateStream(bms, CompressionMode.Decompress, true))
                        {
                            ds.CopyTo(ms);
                        }
                    }
                    catch
                    {
                    }

                    return ms.ToArray();
                }
            }
        }

    }
}
