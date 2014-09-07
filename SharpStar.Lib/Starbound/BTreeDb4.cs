﻿// SharpStar
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Mono;
using SharpStar.Lib.Zlib;

namespace SharpStar.Lib.Starbound
{
    public class BTreeDb4 : SBBF02
    {


        public string Identifier { get; protected set; }

        public int KeySize { get; protected set; }

        public bool AlternateRootNode { get; protected set; }

        public int RootNode { get; protected set; }

        public bool RootNodeIsLeaf { get; protected set; }

        public int OtherRootNode { get; protected set; }

        public bool OtherRootNodeIsLeaf { get; protected set; }

        public byte[] GetBinary(byte[] key)
        {

            if (key.Length != KeySize)
                throw new Exception("Invalid key length!");

            Block block = GetBlock(RootNode);

            while (block is BTreeIndex)
            {

                BTreeIndex index = (BTreeIndex)block;

                int blockNumber = index.GetBlockForKey(key);

                block = GetBlock(blockNumber);

            }

            if (!(block is BTreeLeaf))
                throw new Exception("Did not reach a leaf!");

            return GetLeafValue((BTreeLeaf)block, key);

        }

        public byte[] GetLeafValue(BTreeLeaf leaf, Key key)
        {

            var reader = new LeafReader(this, leaf);

            var unpacked = DataConverter.Unpack("^i", reader.Read(4), 0);

            var numKeys = (int)unpacked[0];

            int discarded;

            for (int i = 0; i < numKeys; i++)
            {

                byte[] curKey = reader.Read(KeySize);

                int length = (int)VLQ.ReadVLQ(reader, out discarded);

                byte[] value = reader.Read(length);

                if (curKey.SequenceEqual(key.TheKey))
                {
                    return value;
                }

            }

            return null;

        }

        public byte[] GetRaw(byte[] key)
        {
            return GetBinary(EncodeKey(key));
        }

        public byte[] Get(byte[] key)
        {
            return GetAsync(key).Result;
        }

        public async Task<byte[]> GetAsync(byte[] key)
        {
            byte[] enc = EncodeKey(key);

            return await ZlibUtils.DecompressAsync(GetBinary(enc));
        }

        public virtual byte[] EncodeKey(byte[] key)
        {
            return key;
        }

        public override void Read(byte[] data)
        {

            base.Read(data);

            using (MemoryStream ms = new MemoryStream(UserHeader))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    string dbFormat = Encoding.UTF8.GetString(reader.ReadBytes(12));

                    if (dbFormat.Replace("\x00", "") != "BTreeDB4")
                        throw new Exception("Expected BTree database!");

                    Identifier = Encoding.UTF8.GetString(reader.ReadBytes(12)).Replace("\x00", "");

                    var fields = DataConverter.Unpack("^ibxibxxxib", reader.ReadBytes(19), 0);

                    KeySize = (int)fields[0];

                    AlternateRootNode = Convert.ToBoolean(fields[1]);

                    if (AlternateRootNode)
                    {
                        RootNode = (int)fields[4];
                        RootNodeIsLeaf = Convert.ToBoolean(fields[5]);
                        OtherRootNode = (int)fields[2];
                        OtherRootNodeIsLeaf = Convert.ToBoolean(fields[3]);
                    }
                    else
                    {
                        RootNode = (int)fields[2];
                        RootNodeIsLeaf = Convert.ToBoolean(fields[3]);
                        OtherRootNode = (int)fields[4];
                        OtherRootNodeIsLeaf = Convert.ToBoolean(fields[5]);
                    }

                }
            }

        }

    }

    public class BTreeIndex : Block
    {

        public static readonly byte[] SIGNATURE = { 0x49, 0x49 };

        public byte Level { get; private set; }

        public int NumKeys { get; private set; }

        public int LeftBlock { get; private set; }

        public List<Key> Keys { get; private set; }

        public List<int> Values { get; private set; }

        public BTreeIndex()
            : base(SIGNATURE)
        {
        }

        public int GetBlockForKey(Key key)
        {

            int pos = -1;

            for (int i = 0; i < Keys.Count; i++)
            {

                if (Keys[i].TheKey.SequenceEqual(key.TheKey))
                {

                    pos = i;

                    break;
                
                }

            }

            return pos == -1 ? Values[0] : Values[pos + 1];

        }

        public override void Read(SBBF02 sbb, int blockIndex)
        {

            base.Read(sbb, blockIndex);

            if (!(sbb is BTreeDb4))
                throw new Exception("Expected BTreeDb4!");

            Keys = new List<Key>();
            Values = new List<int>();

            BTreeDb4 db = (BTreeDb4)sbb;

            var unpacked = DataConverter.Unpack("^Cii", db.Reader.ReadBytes(9), 0);

            Level = (byte)(char)unpacked[0];
            NumKeys = (int)unpacked[1];
            LeftBlock = (int)unpacked[2];

            Values.Add(LeftBlock);

            for (int i = 0; i < NumKeys; i++)
            {

                Key key = sbb.Reader.ReadBytes(db.KeySize);

                var unpacked2 = DataConverter.Unpack("^i", db.Reader.ReadBytes(4), 0);

                int block = (int)unpacked2[0];

                Keys.Add(key);
                Values.Add(block);

            }

        }

    }

    public class BTreeLeaf : Block
    {

        public static readonly byte[] SIGNATURE = { 0x4C, 0x4C };

        public byte[] Data { get; protected set; }

        public int? NextBlock { get; protected set; }

        public BTreeLeaf()
            : base(SIGNATURE)
        {
        }

        public override void Read(SBBF02 sb, int blockIndex)
        {

            base.Read(sb, blockIndex);

            Data = sb.Reader.ReadBytes(sb.BlockSize - 6);

            var unpacked = DataConverter.Unpack("^i", sb.Reader.ReadBytes(4), 0);

            int value = (int)unpacked[0];

            NextBlock = value != -1 ? value : 0;

        }

    }

    public class BTreeRestoredLeaf : BTreeLeaf
    {

        public BlockFree FreeBlock { get; protected set; }

        public BTreeRestoredLeaf(BlockFree freeBlock)
        {
            FreeBlock = freeBlock;
        }

        public override void Read(SBBF02 sb, int blockIndex)
        {

            byte[] data = FreeBlock.RawData.Take(FreeBlock.RawData.Length - 4).ToArray();

            var unpacked = DataConverter.Unpack("^i", data.Skip(data.Length - 4).ToArray(), 0);

            int value = (int)unpacked[0];

            NextBlock = value != -1 ? (int?)value : null;

        }

    }

}
