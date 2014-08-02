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
using SharpStar.Lib.Networking;
using SharpStar.Lib.Starbound;

namespace SharpStar.Lib.Packets
{
    public class ClientContextUpdatePacket : Packet
    {
        public override byte PacketId
        {
            get { return (byte)KnownPacket.ClientContextUpdate; }
        }

        public byte[] Data { get; set; }

        public World World { get; set; }

        public override void Read(IStarboundStream stream)
        {

            Data = stream.ReadUInt8Array();

            if (Data.Length != 0)
            {

                using (MemoryStream ms = new MemoryStream(Data))
                {

                    using (StarboundStream s = new StarboundStream(ms))
                    {

                        byte[] data = s.ReadUInt8Array();

                        if (data.Length > 8)
                        {

                            using (MemoryStream ms2 = new MemoryStream(data))
                            {

                                using (StarboundStream s2 = new StarboundStream(ms2))
                                {

                                    byte bufLength = s2.ReadUInt8();

                                    if (bufLength == 10 && (s2.Length - s2.Position) >= 1000)
                                    {
                                        World = new World();
                                        World.Read(s2.ReadUInt8Array());
                                    }
                                    else if (bufLength == 2)
                                    {

                                        int discarded;

                                        int vlq = (int)s2.ReadVLQ(out discarded);

                                        if (vlq == 4 && s2.Length - s2.Position >= 1000) //World data is at least over 1000 bytes, probably more
                                        {

                                            byte[] unknown1 = s2.ReadUInt8Array(30);

                                            byte[] d = s2.ReadUInt8Array();

                                            World = new World();
                                            World.Read(d);

                                        }

                                    }

                                }

                            }
                        }

                    }

                }

            }

        }

        public override void Write(IStarboundStream stream)
        {
            stream.WriteUInt8Array(Data);
        }
    }
}