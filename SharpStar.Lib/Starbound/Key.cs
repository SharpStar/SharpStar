using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Starbound
{
    public class Key : IEquatable<Key>, IComparable<Key>
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

        public int CompareTo(Key other)
        {

            if (this == other) return 0;
            if (other == null) return -1;

            var len = Math.Min(TheKey.Length, other.TheKey.Length);
            for (var i = 0; i < len; i++)
            {
                var c = TheKey[i].CompareTo(other.TheKey[i]);

                if (c != 0)
                {
                    return c;
                }
            }

            return TheKey.Length.CompareTo(other.TheKey.Length);

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
