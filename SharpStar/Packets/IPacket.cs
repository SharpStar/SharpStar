using SharpStar.Networking;

namespace SharpStar.Packets
{
    public interface IPacket
    {

        byte PacketId { get; }

        bool Ignore { get; set; }

        void Read(StarboundStream stream);

        void Write(StarboundStream stream);

    }
}
