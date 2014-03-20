using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SharpStar.Lib.Server
{
    public class StarboundServer : IDisposable
    {
        private static readonly object ClientLocker = new object();

        private bool _disposed;

        public const int NetworkPort = 21024;
        public const int ClientBufferLength = 1024;
        public const int ProtocolVersion = 636;

        private readonly int _serverPort;

        public TcpListener Listener { get; set; }

        public List<StarboundServerClient> Clients { get; set; }

        public StarboundServer(int listenPort, int serverPort)
        {
            _serverPort = serverPort;

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

                IPEndPoint ipe = (IPEndPoint)socket.RemoteEndPoint;

                Console.WriteLine("Connection from {0}", ipe);

                StarboundClient sc = new StarboundClient(socket, Direction.Client);


                ssc = new StarboundServerClient(sc);
                ssc.Disconnected += (sender, args) =>
                {
                    lock (ClientLocker)
                    {
                        if (Clients.Contains(ssc))
                        {

                            if (ssc.Player != null)
                                Console.WriteLine("Player {0} disconnected", ssc.Player.Name);
                            else
                                Console.WriteLine("{0} disconnected", ipe);

                            Clients.Remove(ssc);

                            ssc.Dispose();
                        }
                    }
                };

                ssc.Connect(_serverPort);

                lock (ClientLocker)
                    Clients.Add(ssc);
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
}