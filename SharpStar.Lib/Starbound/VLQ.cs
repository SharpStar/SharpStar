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
