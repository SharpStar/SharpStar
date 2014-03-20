using System;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public interface IPacketHandler
    {

        Type GetPacketType();

        void Handle(IPacket packet, IClient client);

        void HandleAfter(IPacket packet, IClient client);

    }
}
