using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SharpStar.Lib.Server
{
    public class StarboundServer : IDisposable
    {

        private bool _disposed;

        public const int NetworkPort = 21024;
        public const int ClientBufferLength = 1024;
        public const int ProtocolVersion = 642;

        private readonly int _serverPort;

        #region Server Stuff

        private int bufferSize;

        private int connectedSockets;

        private int numConnections;

        private SocketAsyncEventArgsPool readWritePool;

        private Semaphore semaphoreAcceptedClients;

        private IPEndPoint localEndPoint;

        private BufferManager _bufManager;

        #endregion

        public Socket Listener { get; set; }


        public List<StarboundServerClient> Clients { get; set; }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        public StarboundServer(int listenPort, int serverPort, int maxPlayers)
        {

            Clients = new List<StarboundServerClient>();

            this.connectedSockets = 0;
            this.numConnections = maxPlayers;
            this.bufferSize = 5;

            this.readWritePool = new SocketAsyncEventArgsPool(numConnections);
            this.semaphoreAcceptedClients = new Semaphore(numConnections, numConnections);

            _serverPort = serverPort;

            StarboundServerClient.BufferManager = new BufferManager(this.bufferSize * this.numConnections * 2, this.bufferSize);
            StarboundServerClient.BufferManager.InitBuffer();

            _bufManager = new BufferManager(this.bufferSize * this.numConnections * 2, this.bufferSize);
            _bufManager.InitBuffer();


            localEndPoint = new IPEndPoint(IPAddress.Any, listenPort);


            for (int i = 0; i < this.numConnections; i++)
            {

                SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();

                _bufManager.SetBuffer(readWriteEventArg);

                this.readWritePool.Push(readWriteEventArg);


            }

            this.Listener = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.Listener.NoDelay = true;
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {

            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += OnAcceptCompleted;
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            this.semaphoreAcceptedClients.WaitOne();

            if (!this.Listener.AcceptAsync(acceptEventArg))
            {
                this.ProcessAccept(acceptEventArg);
            }

        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {

            Socket s = e.AcceptSocket;
            s.NoDelay = true;

            if (s.Connected)
            {
                try
                {

                    SocketAsyncEventArgs readEventArgs = this.readWritePool.Pop();

                    if (readEventArgs != null)
                    {

                        Console.WriteLine("Connection from {0}", s.RemoteEndPoint);

                        StarboundClient sc = new StarboundClient(s, Direction.Client);
                        readEventArgs.UserToken = new Token(s, sc);

                        StarboundServerClient ssc = new StarboundServerClient(sc, readEventArgs);


                        ssc.Disconnected += (sender, args) =>
                        {

                            if (Clients.Contains(ssc))
                            {

                                if (ssc.Player != null)
                                    Console.WriteLine("Player {0} disconnected", ssc.Player.Name);
                                else
                                    Console.WriteLine("{0} disconnected", s.RemoteEndPoint);

                                if (ClientDisconnected != null)
                                    ClientDisconnected(this, new ClientDisconnectedEventArgs(ssc));

                                Clients.Remove(ssc);

                                ssc.Dispose();

                            }

                        };

                        ssc.ServerClientConnected += (sender, args) =>
                        {

                            if (ClientConnected != null)
                                ClientConnected(this, new ClientConnectedEventArgs(ssc));

                        };

                        Interlocked.Increment(ref this.connectedSockets);

                        ssc.Connect(_serverPort);

                        Clients.Add(ssc);

                    }
                    else
                    {
                        Console.WriteLine("There are no more available sockets to allocate.");
                    }

                }
                catch (SocketException ex)
                {
                    Token token = e.UserToken as Token;
                    Console.WriteLine("Error when processing data received from {0}:\r\n{1}", token.Connection.RemoteEndPoint, ex.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                // Accept the next connection request.
                this.StartAccept(e);

            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            Token token = e.UserToken as Token;
            this.CloseClientSocket(token, e);
        }

        private void CloseClientSocket(Token token, SocketAsyncEventArgs e)
        {

            this.semaphoreAcceptedClients.Release();
            Interlocked.Decrement(ref this.connectedSockets);

            this.readWritePool.Push(e);

            token.SClient.ForceDisconnect();

            e.Dispose();

        }

        public void Start()
        {

            this.Listener.Bind(localEndPoint);
            this.Listener.Listen(this.numConnections);

            this.StartAccept(null);

        }

        public void Stop()
        {

            foreach (StarboundServerClient client in Clients)
            {
                client.ForceDisconnect();
                client.Dispose();
            }

            this.Listener.Close();

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