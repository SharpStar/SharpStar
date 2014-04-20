using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Starbound
{
    public class Key : IEquatable<Key>
    {

        public byte[] TheKey { get; set; }

        public Key(byte[] key)
        {
            TheKey = key;
        }

        public override int GetHashCode()
        {

            int hashcode = 0;

            foreach (byte b in TheKey)
            {
                hashcode += b.GetHashCode();
            }

            return hashcode;

        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Key);
        }

        public bool Equals(Key other)
        {
            return !ReferenceEquals(other, null) && TheKey.SequenceEqual(other.TheKey);
        }

        public static implicit operator Key(byte[] signature)
        {
            return new Key(signature);
        }

        public static implicit operator byte[](Key key)
        {
            return key.TheKey;
        }

        public static bool operator ==(Key key1, Key key2)
        {

            if (ReferenceEquals(key1, null) || ReferenceEquals(key2, null))
                return false;

            return key1.Equals(key2);
        }

        public static bool operator !=(Key key1, Key key2)
        {
            return !(key1 == key2);
        }

    }
}
