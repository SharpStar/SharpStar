using System;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public interface IPacketHandler
    {

        Type GetPacketType();

        void Handle(IPacket packet, IClient client);

        void HandleAfter(IPacket packet, IClient client);

    }
}
