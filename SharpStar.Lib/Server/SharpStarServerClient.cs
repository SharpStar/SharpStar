﻿// SharpStar
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Packets.Handlers;
using SharpStar.Lib.Security;

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

                return _packetHandlers.ToList();
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

            if (e.SocketError == SocketError.Success && ((AsyncUserToken)e.UserToken).Socket.Connected)
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

                PlayerClient.StartReceive().ContinueWith(t => t.Exception.LogError(), TaskContinuationOptions.OnlyOnFaulted);
                ServerClient.StartReceive().ContinueWith(t => t.Exception.LogError(), TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {

                if (PlayerClient == null)
                    return;

                new SocketException((int)e.SocketError).LogError();

                //simulate the connection process so we can return an error back to the client
                var packetRecv = Observable.FromEventPattern<PacketEventArgs>(p => PlayerClient.PacketReceived += p, p => PlayerClient.PacketReceived -= p);
                var clientConnPacket = (from p in packetRecv where p.EventArgs.Packet.PacketId == (int)KnownPacket.ClientConnect select p);
                var subscribeConn = clientConnPacket.Subscribe(args =>
                {

                    args.EventArgs.Packet.Ignore = true;

                    SharpStarClient client = args.EventArgs.Client;

                    if (client != null)
                    {
                        client.SendPacket(new HandshakeChallengePacket
                        {
                            Claim = String.Empty,
                            Rounds = StarboundConstants.Rounds,
                            Salt = SharpStarSecurity.GenerateSalt()
                        }).ContinueWith(t => t.Exception.LogError(), TaskContinuationOptions.OnlyOnFaulted);
                    }

                }, args => { }, () => { });

                var handshakeRespPacket = (from p in packetRecv where p.EventArgs.Packet.PacketId == (int)KnownPacket.HandshakeResponse select p);
                var subscribeHandshake = handshakeRespPacket.Subscribe(args =>
                {

                    SharpStarClient client = args.EventArgs.Client;

                    args.EventArgs.Packet.Ignore = true;

                    if (client != null)
                    {
                        client.SendPacket(new ConnectionResponsePacket
                        {
                            Success = false,
                            RejectionReason = SharpStarMain.Instance.Config.ConfigFile.ServerOfflineError,
                            ClientId = 1
                        }).ContinueWith(t => t.Exception.LogError(), TaskContinuationOptions.OnlyOnFaulted);
                    }

                }, args => { }, () => { });

                PlayerClient.StartReceive().ContinueWith(t => t.Exception.LogError(), TaskContinuationOptions.OnlyOnFaulted);
                PlayerClient.SendPacket(new ProtocolVersionPacket
                {
                    ProtocolVersion = StarboundConstants.ProtocolVersion
                }).ContinueWith(t => t.Exception.LogError(), TaskContinuationOptions.OnlyOnFaulted);

                Task.Run(() =>
                {
                    Thread.Sleep(5000);

                    subscribeConn.Dispose();
                    subscribeHandshake.Dispose();
                });

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
