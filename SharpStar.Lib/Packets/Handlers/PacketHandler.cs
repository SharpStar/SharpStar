﻿using System;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public abstract class PacketHandler<T> : IPacketHandler where T : Packet, new()
    {
        public abstract void Handle(T packet, StarboundClient client);

        public abstract void HandleAfter(T packet, StarboundClient client);

        private static readonly int _packetId = new T().PacketId;

        public int PacketId
        {
            get
            {
                return _packetId;
            }
        }

        public Type GetPacketType()
        {
            return typeof (T);
        }

        public void Handle(IPacket packet, IClient client)
        {
            if (packet is T)
                Handle((T) packet, (StarboundClient) client);
            else
                throw new Exception(String.Format("Was given packet type {0}, expected {1}", packet.GetType().Name,
                    typeof (T).Name));
        }

        public void HandleAfter(IPacket packet, IClient client)
        {
            if (packet is T)
                HandleAfter((T) packet, (StarboundClient) client);
            else
                throw new Exception(String.Format("Was given packet type {0}, expected {1}", packet.GetType().Name,
                    typeof (T).Name));
        }

    }
}