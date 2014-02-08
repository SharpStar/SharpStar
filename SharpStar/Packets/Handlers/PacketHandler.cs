using System;
using SharpStar.Server;

namespace SharpStar.Packets.Handlers
{
    public abstract class PacketHandler<T> : IPacketHandler where T : IPacket
    {

        public abstract void Handle(T packet, IClient client);

        public abstract void HandleAfter(T packet, IClient client);

        public Type GetPacketType()
        {
            return typeof(T);
        }

        public void Handle(IPacket packet, IClient client)
        {
            if (packet is T)
                Handle((T)packet, client);
            else
                throw new Exception(String.Format("Was given packet type {0}, expected {1}", packet.GetType().Name, typeof(T).Name));
        }

        public void HandleAfter(IPacket packet, IClient client)
        {
            if (packet is T)
                HandleAfter((T)packet, client);
            else
                throw new Exception(String.Format("Was given packet type {0}, expected {1}", packet.GetType().Name, typeof(T).Name));
        }
    }
}
