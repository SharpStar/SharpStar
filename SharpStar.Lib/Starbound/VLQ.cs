using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Starbound
{
    public static class VLQ
    {

        public static ulong ReadVLQ(LeafReader leafReader, out int length)
        {

            ulong value = 0L;

            length = 0;

            while (true)
            {

                byte tmp = leafReader.Read(1)[0];

                value = (value << 7) | (ulong)(tmp & 0x7f);

                length++;

                if ((tmp & 0x80) == 0)
                    break;

            }

            return value;

        }

        public static ulong ReadVLQ(Stream memoryStream, out int length)
        {

            ulong value = 0L;

            length = 0;

            while (true)
            {

                byte[] buf = new byte[1];

                memoryStream.Read(buf, 0, buf.Length);

                byte tmp = buf[0];

                value = (value << 7) | (ulong)(tmp & 0x7f);

                length++;

                if ((tmp & 0x80) == 0)
                    break;

            }

            return value;

        }

    }
}
