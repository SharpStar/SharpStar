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
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.DataTypes
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet)
    /// <summary>
    /// A Variant is a structure used by Starbound that incorporates several different data types.
    /// </summary>
    public class Variant
    {
        public object Value { get; private set; }

        private Variant()
        {
        }

        public Variant(object value)
        {
            if (!(value == null ||
                  value is string ||
                  value is double ||
                  value is bool ||
                  value is ulong ||
                  value is uint ||
                  value is ushort ||
                  value is byte ||
                  value is Variant[] ||
                  value is VariantDict))
            {
                throw new InvalidCastException(string.Format("Variants are unable to represent {0}.", value.GetType()));
            }

            Value = value;
        }


        public static Variant FromStream(IStarboundStream stream)
        {
            var variant = new Variant();
            byte type = stream.ReadUInt8();
            int discarded;
            switch (type)
            {
                case 1:
                    variant.Value = null;
                    break;
                case 2:
                    variant.Value = stream.ReadDouble();
                    break;
                case 3:
                    variant.Value = stream.ReadBoolean();
                    break;
                case 4:
                    variant.Value = stream.ReadVLQ(out discarded);
                    break;
                case 5:
                    variant.Value = stream.ReadString();
                    break;
                case 6:
                    var array = new Variant[stream.ReadVLQ(out discarded)];
                    for (int i = 0; i < array.Length; i++)
                        array[i] = Variant.FromStream(stream);
                    variant.Value = array;
                    break;
                case 7:
                    var dict = new VariantDict();
                    var length = stream.ReadVLQ(out discarded);
                    while (length-- > 0)
                    {
                        dict[stream.ReadString()] = Variant.FromStream(stream);
                    }

                    variant.Value = dict;
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Unknown Variant type: 0x{0:X2}", type));
            }
            return variant;
        }

        public void WriteTo(IStarboundStream stream)
        {
            if (Value == null)
            {
                stream.WriteUInt8(1);
            }
            else if (Value is double)
            {
                stream.WriteInt8(2);
                stream.WriteDouble((double)Value);
            }
            else if (Value is bool)
            {
                stream.WriteInt8(3);
                stream.WriteBoolean((bool)Value);
            }
            else if (Value is ulong)
            {
                stream.WriteInt8(4);
                stream.WriteVLQ((ulong)Value);
            }
            else if (Value is string)
            {
                stream.WriteInt8(5);
                stream.WriteString((string)Value);
            }
            else if (Value is Variant[])
            {
                stream.WriteInt8(6);
                var array = (Variant[])Value;
                stream.WriteVLQ((ulong)array.Length);
                for (int i = 0; i < array.Length; i++)
                    array[i].WriteTo(stream);
            }
            else if (Value is VariantDict)
            {
                stream.WriteInt8(7);
                var dict = (VariantDict)Value;
                stream.WriteVLQ((ulong)dict.Count);
                foreach (var kvp in dict)
                {
                    stream.WriteString(kvp.Key);
                    kvp.Value.WriteTo(stream);
                }
            }
        }

    }
}
