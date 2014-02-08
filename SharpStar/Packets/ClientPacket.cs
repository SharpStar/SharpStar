using SharpStar.Networking;

namespace SharpStar.Packets
{
    public abstract class ClientPacket : IPacket
    {
        public abstract byte PacketId { get; set; }
        public abstract bool Ignore { get; set; }
        public abstract void Read(StarboundStream stream);
        public abstract void Write(StarboundStream stream);
    }
}
