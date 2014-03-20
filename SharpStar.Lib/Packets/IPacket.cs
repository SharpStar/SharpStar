using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Packets
{
    public interface IPacket
    {

        byte PacketId { get; }

        bool Ignore { get; set; }

        void Read(IStarboundStream stream);

        void Write(IStarboundStream stream);

    }
}
