using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Packets.Handlers;

namespace SharpStar.Lib.Server
{
    public class StarboundServer : IDisposable
    {

        private readonly object _clientLocker = new object();

        private bool _disposed;

        public const int NetworkPort = 21024;
        public const int ClientBufferLength = 1024;
        public const int ProtocolVersion = 639;

        private int _clientCtr;

        private readonly int _serverPort;

        public int ServerPort
        {
            get
            {
                return _serverPort;
            }
        }

        public TcpListener Listener { get; set; }

        public List<StarboundServerClient> Clients { get; set; }

        public List<IPacketHandler> DefaultPacketHandlers = new List<IPacketHandler>
        {
            new UnknownPacketHandler(),
            new ProtocolVersionPacketHandler(),
            new ClientConnectPacketHandler(),
            new ClientDisconnectPacketHandler(),
            new HandshakeResponsePacketHandler(),
            new ChatSentPacketHandler(),
            new RequestDropPacketHandler(),
            new WarpCommandPacketHandler(),
            new OpenContainerPacketHandler(),
            new CloseContainerPacketHandler(),
            new DamageNotificationPacketHandler(),
            new ConnectionResponsePacketHandler(),
            new HandshakeChallengePacketHandler(),
            new DisconnectResponsePacketHandler(),
            new ChatReceivedPacketHandler(),
            new EntityInteractResultPacketHandler(),
            new UniverseTimeUpdatePacketHandler(),
            new ClientContextUpdatePacketHandler(),
            new WorldStartPacketHandler(),
            new WorldStopPacketHandler(),
            new TileDamageUpdatePacketHandler(),
            new GiveItemPacketHandler(),
            new EnvironmentUpdatePacketHandler(),
            new EntityCreatePacketHandler(),
            new EntityUpdatePacketHandler(),
            new EntityDestroyPacketHandler(),
            new UpdateWorldPropertiesPacketHandler(),
            new HeartbeatPacketHandler()
        };

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        public StarboundServer(int listenPort, int serverPort)
        {

            _serverPort = serverPort;

            _clientCtr = 0;

            Listener = new TcpListener(new IPEndPoint(IPAddress.Any, listenPort));
            Clients = new List<StarboundServerClient>();
        
        }

        public void Start()
        {
            Listener.Start();
            Listener.BeginAcceptSocket(AcceptClient, null);
        }

        public void Stop()
        {
            foreach (StarboundServerClient client in Clients)
            {
                client.ForceDisconnect();
                client.Dispose();
            }

            Listener.Stop();
        }

        public void AddClient(StarboundServerClient client)
        {
            lock (_clientLocker)
            {
                Clients.Add(client);
            }
        }

        public void RemoveClient(StarboundServerClient client)
        {
            lock (_clientLocker)
            {
                Clients.Remove(client);
            }
        }

        private void AcceptClient(IAsyncResult iar)
        {
            if (_disposed)
                return;

            StarboundServerClient ssc = null;

            try
            {
                Socket socket = Listener.EndAcceptSocket(iar);
                socket.NoDelay = true;

                IPEndPoint ipe = (IPEndPoint)socket.RemoteEndPoint;

                SharpStarLogger.DefaultLogger.Info("Connection from {0}", ipe);

                Interlocked.Increment(ref _clientCtr);

                new Thread(() =>
                {

                    StarboundClient sc = new StarboundClient(socket, Direction.Client);
                    ssc = new StarboundServerClient(sc);
                    ssc.ClientId = _clientCtr;

                    foreach (IPacketHandler packetHandler in DefaultPacketHandlers)
                    {
                        ssc.RegisterPacketHandler(packetHandler);
                    }

                    ssc.Disconnected += (sender, args) =>
                    {
                        lock (_clientLocker)
                        {
                            if (Clients.Contains(ssc))
                            {

                                if (ssc.Player != null)
                                    SharpStarLogger.DefaultLogger.Info("Player {0} disconnected", ssc.Player.Name);
                                else
                                    SharpStarLogger.DefaultLogger.Info("{0} disconnected", ipe);

                                if (ClientDisconnected != null)
                                    ClientDisconnected(this, new ClientDisconnectedEventArgs(ssc));

                                Clients.Remove(ssc);

                            }
                        }
                    };

                    ssc.SClientConnected += (sender, args) =>
                    {
                        if (ClientConnected != null)
                            ClientConnected(this, new ClientConnectedEventArgs(ssc));
                    };

                    ssc.Connect("127.0.0.1", _serverPort);

                    lock (_clientLocker)
                        Clients.Add(ssc);

                }).Start();

            }
            catch (SocketException)
            {

                if (ssc != null)
                {
                    ssc.ForceDisconnect();
                }

            }
            catch (ObjectDisposedException)
            {
                return;   
            }
            catch (Exception)
            {
            }

            Listener.BeginAcceptSocket(AcceptClient, null);

        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            _disposed = true;

            if (disposing)
            {
                Stop();
            }
        }

        ~StarboundServer()
        {
            Dispose(false);
        }

    }

    public class ClientConnectedEventArgs : EventArgs
    {

        public StarboundServerClient ServerClient { get; set; }

        public ClientConnectedEventArgs(StarboundServerClient serverClient)
        {
            ServerClient = serverClient;
        }

    }

    public class ClientDisconnectedEventArgs : EventArgs
    {

        public StarboundServerClient ServerClient { get; set; }

        public ClientDisconnectedEventArgs(StarboundServerClient ssc)
        {
            ServerClient = ssc;
        }

    }

}