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
