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
            get { return 13; }
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