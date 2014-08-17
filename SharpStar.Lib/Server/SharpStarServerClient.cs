using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Packets.Handlers;

namespace SharpStar.Lib.Server
{
    public sealed class SharpStarServerClient
    {

        public DateTime ConnectionTime { get; set; }

        public int ClientId { get; set; }

        public SharpStarClient ServerClient { get; set; }

        public SharpStarClient PlayerClient { get; set; }

        public StarboundPlayer Player { get; set; }

        public event EventHandler<ClientConnectedEventArgs> SClientConnected;

        public List<IPacketHandler> PacketHandlers { get; private set; }

        public SharpStarServerClient(SharpStarClient plrClient)
        {
            PlayerClient = plrClient;
            PlayerClient.Server = this;

            PacketHandlers = new List<IPacketHandler>();
        }

        public void RegisterPacketHandler(IPacketHandler handler)
        {
            if (PacketHandlers.All(p => p.GetType() != handler.GetType()))
                PacketHandlers.Add(handler);
        }

        public void UnregisterPacketHandler(IPacketHandler handler)
        {
            PacketHandlers.Remove(handler);
        }


        public void Connect(IPEndPoint ipe)
        {
            var connectArgs = new SocketAsyncEventArgs();

            var token = new AsyncUserToken();
            token.Socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            connectArgs.RemoteEndPoint = ipe;
            connectArgs.UserToken = token;
            connectArgs.Completed += connectArgs_Completed;

            token.Socket.ConnectAsync(connectArgs);

            SocketError errorCode = connectArgs.SocketError;
            
            if (errorCode != SocketError.Success)
            {
                new SocketException((int)errorCode).LogError();
            }
        }

        private void connectArgs_Completed(object sender, SocketAsyncEventArgs e)
        {

            if (e.SocketError == SocketError.Success)
            {
                ConnectionTime = DateTime.Now;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.RemoteEndPoint = e.RemoteEndPoint;
                args.UserToken = e.UserToken;

                byte[] buffer = new byte[1024];

                args.SetBuffer(buffer, 0, buffer.Length);

                ServerClient = new SharpStarClient(args, Direction.Server);
                ServerClient.Server = this;
                ServerClient.OtherClient = PlayerClient;

                PlayerClient.OtherClient = ServerClient;

                if (SClientConnected != null)
                    SClientConnected(this, new ClientConnectedEventArgs(ServerClient));

                PlayerClient.StartReceive();
                ServerClient.StartReceive();
            }
            else
            {
                new SocketException((int)e.SocketError).LogError();
            }

        }

    }
}
