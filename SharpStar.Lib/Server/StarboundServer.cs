// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Packets.Handlers;

namespace SharpStar.Lib.Server
{
    public class StarboundServer : IServer, IDisposable
    {

        private readonly object _clientLocker = new object();

        private bool _disposed;

        private int _clientCtr;

        private readonly int _serverPort;

        private readonly string _starboundBind = SharpStarMain.Instance.Config.ConfigFile.StarboundBind;
        private readonly string _sharpstarBind = SharpStarMain.Instance.Config.ConfigFile.SharpStarBind;

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
            //new TileDamageUpdatePacketHandler(),
            new GiveItemPacketHandler(),
            new EnvironmentUpdatePacketHandler(),
            new EntityCreatePacketHandler(),
            new EntityUpdatePacketHandler(),
            new EntityDestroyPacketHandler(),
            new UpdateWorldPropertiesPacketHandler(),
            new HeartbeatPacketHandler(),
            new SpawnEntityPacketHandler()
        };

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        public StarboundServer(int listenPort, int serverPort)
        {

            _serverPort = serverPort;

            _clientCtr = 0;

            if (_sharpstarBind == "*")
                Listener = new TcpListener(new IPEndPoint(IPAddress.Any, listenPort));
            else
                Listener = new TcpListener(new IPEndPoint(IPAddress.Parse(_sharpstarBind), listenPort));
            
            Clients = new List<StarboundServerClient>();

        }

        public void Start()
        {
            Listener.Start();
            Listener.BeginAcceptSocket(AcceptClient, null);
        }

        public void Stop()
        {
            foreach (StarboundServerClient client in Clients.ToList())
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

                    ssc.Disconnected += (sender, args) =>
                    {
                        lock (_clientLocker)
                        {
                            if (Clients.Contains(ssc))
                            {

                                if (ssc.Player != null && !string.IsNullOrEmpty(ssc.Player.Name))
                                    SharpStarLogger.DefaultLogger.Info("Player {0} disconnected", ssc.Player.Name);
                                else
                                    SharpStarLogger.DefaultLogger.Info("{0} disconnected", ipe);

                                if (ClientDisconnected != null)
                                    ClientDisconnected(this, new ClientDisconnectedEventArgs(ssc));

                                Clients.Remove(ssc);

                                //ssc.Dispose();
                                //ssc = null;

                            }
                        }
                    };

                    if (ssc != null)
                    {
                        ssc.SClientConnected += (sender, args) =>
                        {
                            foreach (IPacketHandler packetHandler in DefaultPacketHandlers)
                            {
                                ssc.RegisterPacketHandler(packetHandler);
                            }

                            if (ClientConnected != null)
                                ClientConnected(this, new ClientConnectedEventArgs(ssc));
                        };

                        if (!string.IsNullOrEmpty(_starboundBind))
                            ssc.Connect(_starboundBind, _serverPort);
                        else
                            ssc.Connect("127.0.0.1", _serverPort);

                        lock (_clientLocker)
                            Clients.Add(ssc);
                    }

                }).Start();

            }
            catch (SocketException ex)
            {
                ex.LogError();

                if (ssc != null)
                {
                    ssc.PlayerClient.ForceDisconnect();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
            }
            finally
            {
                try
                {
                    Listener.BeginAcceptSocket(AcceptClient, null);
                }
                catch (Exception ex)
                {
                    ex.LogError();
                }
            }

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