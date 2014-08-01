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
    public class LeafReader
    {

        public SBBF02 File { get; protected set; }

        public BTreeLeaf Leaf { get; protected set; }

        public int Offset { get; protected set; }

        public List<int> Visited { get; protected set; }

        public LeafReader(SBBF02 sb, BTreeLeaf leaf)
        {
            File = sb;
            Leaf = leaf;
            Offset = 0;
            Visited = new List<int>(new[] { leaf.Index });
        }

        public virtual byte[] Read(int length)
        {

            int offset = Offset;

            if (offset + length <= Leaf.Data.Length)
            {

                Offset += length;

                return Leaf.Data.Skip(offset).Take(length).ToArray();

            }

            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {

                    byte[] buf = Leaf.Data.Skip(offset).ToArray();

                    writer.Write(buf, 0, buf.Length);

                    int numRead = buf.Length;

                    length -= numRead;

                    while (length > 0)
                    {

                        if (!Leaf.NextBlock.HasValue)
                            break;

                        int nextBlock = Leaf.NextBlock.Value;

                        Visited.Add(nextBlock);

                        Leaf = (BTreeLeaf)File.GetBlock(nextBlock);

                        byte[] buf2 = Leaf.Data.Take(length).ToArray();

                        writer.Write(buf2, 0, buf2.Length);

                        numRead = buf2.Length;
                        length -= numRead;


                    }

                    Offset = numRead;

                    data = ms.ToArray();

                }

            }

            return data;

        }



    }
}
