using System;
using System.IO;
using System.Text;
using SharpStar.Lib.DataTypes;
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

        public CelestialLog CelestialLog { get; set; }

        public World World { get; set; }

        public ClientContextUpdatePacket()
        {
            Data = new byte[0];
        }

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

                                    if (bufLength == 10)
                                    {
                                        World = new World();
                                        World.Read(s2.ReadUInt8Array());
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