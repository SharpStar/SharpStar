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
using System.IO;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Networking;
using SharpStar.Lib.DataTypes;

namespace SharpStar.Lib.Packets
{
    public class EntityUpdatePacket : Packet
    {
        public override byte PacketId
        {
            get { return (byte)KnownPacket.EntityUpdate; }
        }

        public long EntityId { get; set; }

        public byte[] Unknown { get; set; }

        public VariantDict Data { get; set; }

        public override void Read(IStarboundStream stream)
        {
            EntityId = stream.ReadSignedVLQ();
            Unknown = stream.ReadUInt8Array((int)(stream.Length - stream.Position));

            //using (MemoryStream ms = new MemoryStream(Unknown))
            //{
            //    using (StarboundStream ss = new StarboundStream(ms))
            //    {
            //        while ((ss.Length - ss.Position) > 0 && ss.ReadUInt8() != 2)
            //        {
            //        }

            //        if (ss.Length - ss.Position == 0)
            //            return;

            //        if (ss.ReadUInt8() != 7)
            //            return;

            //        ss.Seek(-1, SeekOrigin.Current);

            //        try
            //        {
            //            Variant var = ss.ReadVariant();
            //            Data = (VariantDict)var.Value;
            //        }
            //        catch (Exception e)
            //        {
            //            e.LogError();
            //        }

            //    }
            //}

        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteSignedVLQ(EntityId);
            stream.WriteUInt8Array(Unknown, false);
        }
    }
}