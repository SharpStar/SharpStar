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
using System.Text;
using System.Threading;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Packets.Handlers;

namespace SharpStar.Lib.Server
{
    public sealed class SharpStarServer : IServer, IDisposable
    {

        private Socket listenSocket;
        private int m_numConnectedSockets;

        private IPEndPoint sbServerEndPoint;

        private List<SharpStarServerClient> _clients;

        private readonly string _starboundBind = SharpStarMain.Instance.Config.ConfigFile.StarboundBind;

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public List<SharpStarServerClient> Clients
        {
            get
            {
                return _clients.ToList();
            }
        }

        public static List<IPacketHandler> DefaultPacketHandlers = new List<IPacketHandler>
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

        public SharpStarServer(int sbPort, int numConnections)
        {
            m_numConnectedSockets = 0;

            if (!string.IsNullOrEmpty(_starboundBind))
            {
                SharpStarLogger.DefaultLogger.Info("Starbound is bound to {0}", _starboundBind);

                sbServerEndPoint = new IPEndPoint(IPAddress.Parse(_starboundBind), sbPort);
            }
            else
            {
                sbServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sbPort);
            }

            _clients = new List<SharpStarServerClient>();

            Init();
        }

        public void Init()
        {
            //    m_bufferManager.InitBuffer();

            //for (int i = 0; i < m_numConnections; i++)
            //{
            //    SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
            //    readWriteEventArg.UserToken = new AsyncUserToken();

            //    m_bufferManager.SetBuffer(readWriteEventArg);

            //    m_readWritePool.Push(readWriteEventArg);
            //}
        }

        public void Start(IPEndPoint localEndPoint)
        {
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(100);

            StartAccept(null);
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            try
            {
                if (acceptEventArg == null)
                {
                    acceptEventArg = new SocketAsyncEventArgs();
                    acceptEventArg.Completed += AcceptEventArg_Completed;
                }
                else
                {
                    acceptEventArg.AcceptSocket = null;
                }

                bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
                if (!willRaiseEvent)
                {
                    ProcessAccept(acceptEventArg);
                }
            }
            catch (Exception ex)
            {
                ex.LogError();
            }
        }

        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs();
            readEventArgs.UserToken = new AsyncUserToken();

            byte[] buffer = new byte[1024];
            readEventArgs.SetBuffer(buffer, 0, buffer.Length);

            ((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;

            if (!e.AcceptSocket.Connected)
            {
                StartAccept(e);

                return;
            }

            SharpStarLogger.DefaultLogger.Info("Connection from {0}", e.AcceptSocket.RemoteEndPoint);

            Interlocked.Increment(ref m_numConnectedSockets);

            StartAccept(e);

            try
            {
                SharpStarClient client = new SharpStarClient(readEventArgs, Direction.Client);
                client.InternalClientDisconnected += PlayerClient_ClientDisconnected;

                SharpStarServerClient ssc = new SharpStarServerClient(client);
                ssc.SClientConnected += ssc_SClientConnected;
                ssc.ClientId = m_numConnectedSockets;

                foreach (IPacketHandler packetHandler in DefaultPacketHandlers)
                {
                    ssc.RegisterPacketHandler(packetHandler);
                }

                ssc.Connect(sbServerEndPoint);
            }
            catch (Exception ex)
            {
                ex.LogError();
            }
        }

        void ssc_SClientConnected(object sender, ClientConnectedEventArgs e)
        {
            SharpStarServerClient ssc = e.Client.Server;

            _clients.Add(ssc);

            if (ClientConnected != null)
                ClientConnected(this, new ClientConnectedEventArgs(ssc.PlayerClient));

            ssc.ServerClient.InternalClientDisconnected += ServerClient_ClientDisconnected;
        }

        private void ServerClient_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {

            e.Client.InternalClientDisconnected -= ServerClient_ClientDisconnected;

            if (e.Client.Server != null)
            { 
                _clients.Remove(e.Client.Server);
            }

            try
            {
                if (e.Client.Server != null && e.Client.Server.PlayerClient != null)
                    e.Client.Server.PlayerClient.ForceDisconnect();

                e.Client.Dispose();

                if (e.Client.Server != null)
                    e.Client.Server.Dispose();

            }
            catch
            {
            }
            finally
            {
                e.Client.Server = null;
                e.Client = null;
            }

        }

        void PlayerClient_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {

            e.Client.InternalClientDisconnected -= PlayerClient_ClientDisconnected;

            if (e.Client.Server != null && e.Client.Server.PlayerClient != null)
            {

                if (e.Client.Server.Player == null)
                {
                    SharpStarLogger.DefaultLogger.Info("{0} has disconnected", e.Client.Server.PlayerClient.RemoteEndPoint);
                }
                else
                {
                    SharpStarLogger.DefaultLogger.Info("Player {0} ({1}) has disconnected", e.Client.Server.Player.Name, e.Client.Server.PlayerClient.RemoteEndPoint);
                }

            }

            if (e.Client.Server != null)
            {
                _clients.Remove(e.Client.Server);
            }

            try
            {
                if (e.Client.Server != null && e.Client.Server.ServerClient != null)
                {
                    e.Client.Server.ServerClient.ForceDisconnect();
                }

                e.Client.Dispose();
            }
            catch
            {
            }

        }


        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            try
            {
                foreach (SharpStarServerClient ssc in _clients.ToList())
                {
                    ssc.PlayerClient.ForceDisconnect();
                    ssc.ServerClient.ForceDisconnect();

                    ssc.PlayerClient.Dispose();
                    ssc.ServerClient.Dispose();
                }
            }
            catch
            {
            }

            try
            {
                listenSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }
            finally
            {
                listenSocket.Close();
                _clients.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }

            listenSocket = null;
        }

        ~SharpStarServer()
        {
            Dispose(false);
        }

    }

    public class ClientConnectedEventArgs : EventArgs
    {

        public SharpStarClient Client { get; set; }

        public ClientConnectedEventArgs(SharpStarClient client)
        {
            Client = client;
        }

    }

    public class ClientDisconnectedEventArgs : EventArgs
    {

        public SharpStarClient Client { get; set; }

        public ClientDisconnectedEventArgs(SharpStarClient client)
        {
            Client = client;
        }

    }

}
