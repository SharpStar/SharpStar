using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SharpStar.Packets.Handlers;

namespace SharpStar.Server
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

            try
            {

                Socket socket = Listener.EndAcceptSocket(iar);

                Console.WriteLine("Connection from {0}", socket.RemoteEndPoint);

                StarboundClient sc = new StarboundClient(socket);
                sc.RegisterPacketHandler(new UnknownPacketHandler());
                sc.RegisterPacketHandler(new ClientConnectPacketHandler());
                sc.RegisterPacketHandler(new ChatSentPacketHandler());
                sc.RegisterPacketHandler(new RequestDropPacketHandler());
                sc.RegisterPacketHandler(new WarpCommandPacketHandler());
                sc.RegisterPacketHandler(new OpenContainerPacketHandler());
                sc.RegisterPacketHandler(new CloseContainerPacketHandler());
                sc.RegisterPacketHandler(new DamageNotificationPacketHandler());


                StarboundServerClient ssc = new StarboundServerClient(sc);
                ssc.Disconnected += (sender, args) =>
                {

                    lock (ClientLocker)
                    {
                        if (Clients.Contains(ssc))
                        {

                            Console.WriteLine("Player {0} disconnected", ssc.Player.Name);


                            Clients.Remove(ssc);

                            ssc.Dispose();

                        }
                    }

                };

                ssc.ServerClient.RegisterPacketHandler(new UnknownPacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new ConnectionResponsePacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new HandshakeChallengePacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new DisconnectResponsePacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new ChatReceivedPacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new EntityInteractResultPacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new UniverseTimeUpdatePacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new ClientContextUpdatePacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new WorldStartPacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new TileDamageUpdatePacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new GiveItemPacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new EntityCreatePacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new EntityUpdatePacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new EntityDestroyPacketHandler());
                ssc.ServerClient.RegisterPacketHandler(new UpdateWorldPropertiesPacketHandler());

                ssc.Connect(_serverPort);

                lock (ClientLocker)
                    Clients.Add(ssc);

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
