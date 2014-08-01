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

            if (formatStr != "SBBF02")
            {
                throw new Exception("Invalid format! (" + formatStr + ")");
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
