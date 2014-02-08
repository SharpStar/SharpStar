using System;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public abstract class ServerPacketHandler<T> : PacketHandler<T> where T : ServerPacket
    {

        public abstract void Handle(T packet, StarboundClient client);

        public abstract void HandleAfter(T packet, StarboundClient client);

        public override void Handle(T packet, IClient client)
        {
            if (client is StarboundClient)
                Handle(packet, (StarboundClient)client);
            else
                throw new Exception("Expected StarboundClient, got " + client.GetType().Name);
        }

        public override void HandleAfter(T packet, IClient client)
        {
            if (client is StarboundClient)
                HandleAfter(packet, (StarboundClient)client);
            else
                throw new Exception("Expected StarboundClient, got " + client.GetType().Name);
        }

    }
}
