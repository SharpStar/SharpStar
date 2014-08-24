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
    public class SharpStarServerClient : IDisposable
    {

        public DateTime ConnectionTime { get; set; }

        public int ClientId { get; set; }

        public SharpStarClient ServerClient { get; set; }

        public SharpStarClient PlayerClient { get; set; }

        public StarboundPlayer Player { get; set; }

        public event EventHandler<ClientConnectedEventArgs> SClientConnected;

        private List<IPacketHandler> _packetHandlers;

        public List<IPacketHandler> PacketHandlers
        {
            get
            {
                if (_packetHandlers == null)
                    _packetHandlers = new List<IPacketHandler>();

                return _packetHandlers;
            }
        }

        public SharpStarServerClient(SharpStarClient plrClient)
        {
            PlayerClient = plrClient;
            PlayerClient.Server = this;

            _packetHandlers = new List<IPacketHandler>();
        }

        public void RegisterPacketHandler(IPacketHandler handler)
        {
            if (PacketHandlers.All(p => p.GetType() != handler.GetType()))
                _packetHandlers.Add(handler);
        }

        public void UnregisterPacketHandler(IPacketHandler handler)
        {
            _packetHandlers.Remove(handler);
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

            e.Dispose();

        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Player != null)
                    Player.Dispose();
                
                PacketHandlers.Clear();
            }

            _packetHandlers = null;
            Player = null;
        }

    }
}
