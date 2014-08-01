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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Starbound
{
    public class BlockSignature : IEquatable<BlockSignature>
    {

        public byte[] Signature { get; set; }

        public BlockSignature(byte[] signature)
        {
            Signature = signature;
        }

        public override int GetHashCode()
        {

            int hashcode = 0;

            foreach (byte b in Signature)
            {
                hashcode += b.GetHashCode();
            }

            return hashcode;

        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BlockSignature);
        }

        public bool Equals(BlockSignature other)
        {
            return other != null && Signature.SequenceEqual(other.Signature);
        }

        public static implicit operator BlockSignature(byte[] signature)
        {
            return new BlockSignature(signature);
        }

        public static implicit operator byte[](BlockSignature signature)
        {
            return signature.Signature;
        }

    }
}
