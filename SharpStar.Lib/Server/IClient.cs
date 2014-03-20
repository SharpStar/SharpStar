using System.Collections.Concurrent;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Packets.Handlers;

namespace SharpStar.Lib.Server
{
    public interface IClient
    {

        ConcurrentQueue<IPacket> PacketQueue { get; set; }

        void RegisterPacketHandler(IPacketHandler packetHandler);

        void UnregisterPacketHandler(IPacketHandler handler);

        void ForceDisconnect();

        void FlushPackets();

    }
}
