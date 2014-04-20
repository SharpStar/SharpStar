using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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

        public TcpListener Listener { get; set; }

        public List<StarboundServerClient> Clients { get; set; }

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

                Console.WriteLine("Connection from {0}", ipe);

                Interlocked.Increment(ref _clientCtr);

                new Thread(() =>
                {

                    StarboundClient sc = new StarboundClient(socket, Direction.Client);
                    ssc = new StarboundServerClient(sc);
                    ssc.ClientId = _clientCtr;

                    ssc.Disconnected += (sender, args) =>
                    {
                        lock (_clientLocker)
                        {
                            if (Clients.Contains(ssc))
                            {

                                if (ssc.Player != null)
                                    Console.WriteLine("Player {0} disconnected", ssc.Player.Name);
                                else
                                    Console.WriteLine("{0} disconnected", ipe);

                                if (ClientDisconnected != null)
                                    ClientDisconnected(this, new ClientDisconnectedEventArgs(ssc));

                                Clients.Remove(ssc);

                                ssc.Dispose();
                            }
                        }
                    };

                    ssc.SClientConnected += (sender, args) =>
                    {

                        if (ClientConnected != null)
                            ClientConnected(this, new ClientConnectedEventArgs(ssc));

                    };

                    ssc.Connect(_serverPort);

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
            catch (Exception)
            {
            }
            finally
            {
                Listener.BeginAcceptSocket(AcceptClient, null);
            }
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
                Stop();
            }

            _disposed = true;
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