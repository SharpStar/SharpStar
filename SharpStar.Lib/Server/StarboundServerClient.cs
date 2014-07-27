using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using SharpStar.Lib.Entities;
using System.Threading;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Packets.Handlers;

namespace SharpStar.Lib.Server
{
    public class StarboundServerClient : IDisposable
    {

        public bool Connected { get; private set; }

        public int ClientId { get; set; }

        public TcpClient ServerTcpClient { get; set; }


        public event EventHandler<ClientConnectedEventArgs> SClientConnected;

        public event EventHandler<ClientDisconnectedEventArgs> Disconnected;


        public StarboundClient ServerClient { get; set; }

        public StarboundClient PlayerClient { get; set; }

        public StarboundPlayer Player { get; set; }

        public DateTime ConnectionTime { get; private set; }


        private volatile bool _disconnectEventCalled;

        private readonly object _locker = new object();


        public StarboundServerClient(StarboundClient plrClient)
        {

            Player = new StarboundPlayer();

            _disconnectEventCalled = false;

            PlayerClient = plrClient;
            PlayerClient.Server = this;

            ServerTcpClient = new TcpClient { NoDelay = true };

            ServerClient = new StarboundClient(ServerTcpClient.Client, Direction.Server);
            ServerClient.OtherClient = PlayerClient;
            ServerClient.Server = this;

            PlayerClient.OtherClient = ServerClient;

            Connected = false;

        }

        public void Connect(string host, int port)
        {

            if (ServerTcpClient != null && ServerTcpClient.Client.IsConnected())
            {
                ServerTcpClient.Client.Disconnect(true);
                ServerTcpClient = null;
            }

            if (ServerTcpClient == null)
            {

                ServerTcpClient = new TcpClient { NoDelay = true };

                ServerClient.Dispose();
                ServerClient = new StarboundClient(ServerTcpClient.Client, Direction.Server)
                {
                    OtherClient = PlayerClient,
                    Server = this
                };

                PlayerClient.PacketReader = null;
                PlayerClient.OtherClient = ServerClient;

            }

            ServerTcpClient.BeginConnect(host, port, ServerClientConnected, null);

            ConnectionTime = DateTime.Now;

        }

        public void RegisterPacketHandler(IPacketHandler handler)
        {
            PlayerClient.RegisterPacketHandler(handler);
            ServerClient.RegisterPacketHandler(handler);
        }

        public void UnregisterPacketHandler(IPacketHandler handler)
        {
            PlayerClient.UnregisterPacketHandler(handler);
            ServerClient.UnregisterPacketHandler(handler);
        }

        private void ServerClientConnected(IAsyncResult iar)
        {
            try
            {

                ServerTcpClient.EndConnect(iar);
                Connected = true;

                PlayerClient.PacketReader = new PacketReader();
                PlayerClient.PacketQueue = new ConcurrentQueue<IPacket>();

                if (SClientConnected != null)
                    SClientConnected(this, new ClientConnectedEventArgs(this));

                ServerClient.StartReceiving();
                PlayerClient.StartReceiving();

            }
            catch (Exception)
            {
                ForceDisconnect();
            }
        }

        public void ForceDisconnect()
        {

            Connected = false;

            try
            {
                if (ServerClient != null)
                {
                    if (ServerClient.Socket != null)
                    {
                        ServerClient.Socket.Disconnect(true);
                    }

                    ServerClient.Dispose();
                }

                if (PlayerClient != null)
                {
                    if (PlayerClient.Socket != null)
                    {
                        PlayerClient.Socket.Disconnect(true);
                    }

                    PlayerClient.Dispose();
                }

            }
            catch (Exception)
            {
            }
            finally
            {
                lock (_locker)
                {
                    if (Disconnected != null && !_disconnectEventCalled)
                        Disconnected(this, new ClientDisconnectedEventArgs(this));

                    _disconnectEventCalled = true;
                }
            }
        }

        #region Disposal

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //ForceDisconnect();
            }

            PlayerClient = null;
            ServerClient = null;
        }

        #endregion
    }

}