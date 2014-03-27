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
