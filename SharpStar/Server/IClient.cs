using SharpStar.Packets;
using SharpStar.Packets.Handlers;

namespace SharpStar.Server
{
    public interface IClient
    {

        void RegisterPacketHandler<T>(PacketHandler<T> packetHandler) where T : IPacket;

        void UnregisterPacketHandler<T>(PacketHandler<T> handler) where T : IPacket;

        void ForceDisconnect();

    }
}
