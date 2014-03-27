using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono;

namespace SharpStar.Lib.Starbound
{
    public class SBBF02 : IDisposable
    {

        public int HeaderSize { get; protected set; }

        public int BlockSize { get; protected set; }

        public bool FreeBlockDirty { get; protected set; }

        public int FreeBlock { get; protected set; }

        public byte[] UserHeader { get; protected set; }

        public BinaryReader Reader { get; protected set; }

        public Block GetBlock(int blockIndex)
        {

            Reader.BaseStream.Seek(HeaderSize + BlockSize * blockIndex, SeekOrigin.Begin);

            byte[] signature = Block.GetBlockSignature(this);

            if (!Block.BlockSignatures.ContainsKey(signature))
                throw new Exception("Signature not implemented!");

            Type sigType = Block.BlockSignatures[signature];

            Block block = (Block)Activator.CreateInstance(sigType);
            block.Read(this, blockIndex);

            return block;

        }

        public virtual void Read(byte[] data)
        {

            MemoryStream ms = new MemoryStream(data);

            Reader = new BinaryReader(ms);

            char[] format = Reader.ReadChars(6);

            string formatStr = string.Join("", format).Trim();

            Console.WriteLine("FORMAT: " + formatStr);

            if (formatStr != "SBBF02")
            {
                throw new Exception("Invalid format!");
            }

            var unpacked = DataConverter.Unpack("^iibi", Reader.ReadBytes(13), 0);

            HeaderSize = (int)unpacked[0];
            BlockSize = (int)unpacked[1];
            FreeBlockDirty = Convert.ToBoolean(unpacked[2]);
            FreeBlock = (int)unpacked[3];

            ms.Seek(32, SeekOrigin.Begin);

            UserHeader = Reader.ReadBytes(HeaderSize - 32);

        }

        public void Open(string file)
        {

            if (!File.Exists(file))
                throw new FileNotFoundException();

            byte[] dat = File.ReadAllBytes(file);

            Read(dat);

        }

        public void Dispose()
        {

            Dispose(true);

            GC.SuppressFinalize(this);

        }

        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                Reader.Close();
                Reader.Dispose();
            }

            UserHeader = null;
            Reader = null;

        }

        ~SBBF02()
        {
            Dispose(false);
        }

    }
}
