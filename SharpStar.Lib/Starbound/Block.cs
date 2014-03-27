using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Mono;

namespace SharpStar.Lib.Starbound
{
    public abstract class Block
    {

        public static Dictionary<BlockSignature, Type> BlockSignatures { get; protected set; }

        static Block()
        {

            BlockSignatures = new Dictionary<BlockSignature, Type>();

            BlockSignatures.Add(new BlockSignature(BlockFree.SIGNATURE), typeof(BlockFree));
            BlockSignatures.Add(new BlockSignature(BTreeIndex.SIGNATURE), typeof(BTreeIndex));
            BlockSignatures.Add(new BlockSignature(BTreeLeaf.SIGNATURE), typeof(BTreeLeaf));
        
        }

        public int Index { get; protected set; }

        public byte[] Signature { get; protected set; }

        protected Block(byte[] signature)
        {
            Signature = signature;
        }

        public static byte[] GetBlockSignature(SBBF02 sbb)
        {

            byte[] signature = new byte[2];

            sbb.Reader.Read(signature, 0, signature.Length);

            sbb.Reader.BaseStream.Seek(-signature.Length, SeekOrigin.Current);

            return signature;

        }

        public virtual void Read(SBBF02 sbb, int blockIndex)
        {

            byte[] signature = GetBlockSignature(sbb);

            sbb.Reader.BaseStream.Seek(signature.Length, SeekOrigin.Current);

            if (signature[0] == '\x00' && signature[1] == '\x00')
            {
                throw new Exception();
            }

            if (!Signature.SequenceEqual(signature))
                throw new Exception("Signatures don't match!");

            Index = blockIndex;

        }

    }

    public class BlockFree : Block
    {

        public static readonly byte[] SIGNATURE = { 0x46, 0x46 };

        public byte[] RawData { get; private set; }

        public int Value { get; private set; }

        public int NextFreeBlock { get; private set; }

        public BlockFree()
            : base(SIGNATURE)
        {
        }

        public override void Read(SBBF02 sb, int blockIndex)
        {

            base.Read(sb, blockIndex);

            RawData = sb.Reader.ReadBytes(sb.BlockSize - 2);

            var unpacked = DataConverter.Unpack("^i", RawData.Take(4).ToArray(), 0);

            Value = (int)unpacked[0];
            NextFreeBlock = Value != -1 ? Value : 0;

        }

    }

}
