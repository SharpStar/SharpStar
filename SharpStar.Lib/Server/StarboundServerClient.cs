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
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using SharpStar.Lib.Entities;
using System.Threading;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Logging;
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

        private readonly object _eventLocker = new object();

        private bool _connecting;


        public StarboundServerClient(StarboundClient plrClient)
        {
            Player = new StarboundPlayer();

            _disconnectEventCalled = false;
            _connecting = false;

            PlayerClient = plrClient;
            PlayerClient.Server = this;

            Connected = false;
        }

        public void Connect(string host, int port)
        {
            _connecting = true;

            PlayerClient.PacketReader = null;
            PlayerClient.OtherClient = null;

            try
            {
                if (ServerTcpClient != null)
                {

                    if (ServerClient != null)
                    {
                        ServerClient.OtherClient = null;
                        ServerClient.Dispose();
                    }

                    ServerTcpClient.Client.Disconnect(true);
                }
            }
            catch
            {
            }
            finally
            {
                ServerTcpClient = null;
            }

            if (ServerTcpClient == null)
            {

                ServerTcpClient = new TcpClient { NoDelay = true };

                ServerClient = new StarboundClient(ServerTcpClient.Client, Direction.Server)
                {
                    OtherClient = PlayerClient,
                    Server = this
                };

                PlayerClient.PacketReader = null;
                PlayerClient.OtherClient = ServerClient;

            }

            lock (_eventLocker)
            {
                _disconnectEventCalled = false;
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
                _connecting = false;

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

            //try
            //{
            //    if (ServerClient != null)
            //    {
            //        if (ServerClient.Socket != null)
            //        {
            //            ServerClient.Socket.Disconnect(true);
            //        }

            //        ServerClient.Dispose();
            //    }

            //}
            //catch (Exception)
            //{
            //}

            //try
            //{
            //    if (PlayerClient != null)
            //    {
            //        if (PlayerClient.Socket != null)
            //        {
            //            PlayerClient.Socket.Disconnect(true);
            //        }

            //        PlayerClient.Dispose();
            //    }
            //}
            //catch (Exception)
            //{
            //}


            lock (_eventLocker)
            {
                try
                {
                    if (Disconnected != null && !_disconnectEventCalled)
                        Disconnected(this, new ClientDisconnectedEventArgs(this));

                    _disconnectEventCalled = true;
                }
                catch
                {
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
            Player = null;
        }

        ~StarboundServerClient()
        {
            Dispose(false);
        }

        #endregion
    }

}