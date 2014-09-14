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
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zlib;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Networking;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Packets.Handlers;
using SharpStar.Lib.Zlib;

namespace SharpStar.Lib.Server
{
    public sealed class SharpStarClient : IClient, IDisposable
    {

        public Socket Socket { get; set; }

        public IPEndPoint RemoteEndPoint { get; set; }

        private SocketAsyncEventArgs readEventArgs;

        public Direction Direction { get; private set; }

        public SharpStarClient OtherClient { get; set; }

        public SharpStarServerClient Server { get; set; }


        public PacketReader PacketReader;

        public event EventHandler<PacketEventArgs> PacketReceived;

        public event EventHandler<PacketEventArgs> AfterPacketReceived;

        public event EventHandler<PacketEventArgs> SendingPacket;

        internal event EventHandler<ClientDisconnectedEventArgs> InternalClientDisconnected;

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        public ConcurrentQueue<IPacket> PacketQueue { get; set; }

        private IDisposable heartbeatChecker;

        private readonly object closeLocker = new object();

        private long connected;

        public bool Connected
        {
            get
            {
                return Convert.ToBoolean(Interlocked.Read(ref connected));
            }
        }

        public SharpStarClient(SocketAsyncEventArgs eventArgs, Direction dir)
        {
            readEventArgs = eventArgs;
            readEventArgs.Completed += IO_Completed;
            Direction = dir;
            PacketReader = new PacketReader();
            PacketQueue = new ConcurrentQueue<IPacket>();
        }

        public async Task StartReceive()
        {
            connected = Convert.ToInt32(true);

            AsyncUserToken token = (AsyncUserToken)readEventArgs.UserToken;

            Socket = token.Socket;
            Socket.NoDelay = true;

            RemoteEndPoint = (IPEndPoint)Socket.RemoteEndPoint;

            if (Direction == Direction.Client)
            {
                var afterPacket = Observable.FromEventPattern<PacketEventArgs>(p => AfterPacketReceived += p, p => AfterPacketReceived -= p);
                var heartbeatPacket = (from p in afterPacket where p.EventArgs.Packet.PacketId == (int)KnownPacket.Heartbeat select p).Timeout(TimeSpan.FromMinutes(1));
                heartbeatChecker = heartbeatPacket.Subscribe(e => { }, e =>
                {
                    if (Server != null && Server.Player != null)
                        SharpStarLogger.DefaultLogger.Warn("Did not receive a heartbeat packet in a certain amount of time from player {0}. Kicking client!", Server.Player.Name);
                    else
                        SharpStarLogger.DefaultLogger.Warn("Did not receive a heartbeat packet in a certain amount of time. Kicking client!");

                    ForceDisconnect();

                    if (OtherClient != null)
                        OtherClient.ForceDisconnect();
                }, () => { });
            }

            bool willRaiseEvent = token.Socket.ReceiveAsync(readEventArgs);

            if (!willRaiseEvent)
            {
                await ProcessReceive(readEventArgs);
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        ProcessReceive(e).ContinueWith(t => t.Exception.LogError(), TaskContinuationOptions.OnlyOnFaulted);
                        break;
                    case SocketAsyncOperation.Send:
                        e.Completed -= IO_Completed;
                        e.Dispose();
                        //ProcessSend(e);
                        break;
                    default:
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
            catch (Exception ex)
            {
                ex.LogError();

                CloseClientSocket(readEventArgs);
            }
        }

        private async Task ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                PacketReader.NetworkBuffer = new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred);

                var packets = PacketReader.UpdateBuffer(true);
                foreach (IPacket packet in packets)
                {

                    try
                    {

                        if (Server == null)
                            break;

                        EventHandler<PacketEventArgs> packetArgs = PacketReceived;
                        if (packetArgs != null)
                            packetArgs(this, new PacketEventArgs(this, packet));

                        var handlers = Server.PacketHandlers;

                        foreach (IPacketHandler handler in handlers)
                        {
                            if (packet.PacketId == handler.PacketId)
                                await handler.Handle(packet, this);
                        }

                        await SharpStarMain.Instance.PluginManager.CallEvent(packet, OtherClient);

                        if (!packet.Ignore && OtherClient != null)
                            await OtherClient.SendPacket(packet);

                        foreach (IPacketHandler handler in handlers)
                        {
                            if (packet.PacketId == handler.PacketId)
                                await handler.HandleAfter(packet, this);
                        }

                        EventHandler<PacketEventArgs> afterPacketArgs = AfterPacketReceived;
                        if (afterPacketArgs != null)
                            afterPacketArgs(this, new PacketEventArgs(this, packet));

                        await SharpStarMain.Instance.PluginManager.CallEvent(packet, OtherClient, true);
                    }
                    catch (Exception ex)
                    {
                        ex.LogError();
                    }
                }

                try
                {
                    bool willRaiseEvent = token.Socket.ReceiveAsync(e);

                    if (!willRaiseEvent)
                    {
                        await ProcessReceive(e);
                    }
                }
                catch
                {
                }

            }
            else
            {
                CloseClientSocket(e);
            }

        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
            }
            else
            {
                CloseClientSocket(e);
            }

            e.Dispose();
        }

        public async Task SendPacket(IPacket packet)
        {

            EventHandler<PacketEventArgs> sendingPacket = SendingPacket;
            if (sendingPacket != null)
                sendingPacket(this, new PacketEventArgs(this, packet));

            PacketQueue.Enqueue(packet);
            await FlushPackets();
        }

        public void ForceDisconnect()
        {
            this.CloseClientSocket(readEventArgs);
        }

        public async Task FlushPackets()
        {

            while (PacketQueue.Count > 0)
            {
                try
                {
                    IPacket next;

                    if (!PacketQueue.TryDequeue(out next))
                        continue;

                    if (!next.IsReceive)
                    {
                        foreach (var handler in Server.PacketHandlers)
                        {
                            if (next.PacketId == handler.PacketId)
                                await handler.Handle(next, OtherClient);
                        }
                    }

                    if (next.Ignore)
                        continue;

                    var stream = new StarboundStream();
                    next.Write(stream);

                    byte[] buffer = stream.ToArray();
                    bool compressed = stream.Length >= 512;

                    if (compressed)
                    {
                        buffer = ZlibStream.CompressBuffer(buffer);
                    }

                    stream.Dispose();

                    int length = compressed ? -buffer.Length : buffer.Length;

                    var finalStream = new StarboundStream();

                    finalStream.WriteUInt8(next.PacketId);
                    finalStream.WriteSignedVLQ(length);
                    finalStream.Write(buffer, 0, buffer.Length);

                    byte[] toSend = finalStream.ToArray();

                    finalStream.Dispose();

                    if (Socket == null)
                        return;

                    var token = new AsyncUserToken();
                    token.Socket = Socket;

                    SocketAsyncEventArgs writeArgs = new SocketAsyncEventArgs();
                    writeArgs.RemoteEndPoint = Socket.RemoteEndPoint;
                    writeArgs.UserToken = token;
                    writeArgs.SetBuffer(toSend, 0, toSend.Length);
                    writeArgs.Completed += IO_Completed;

                    Socket.SendAsync(writeArgs);

                }
                catch
                {
                    CloseClientSocket(readEventArgs);
                }
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            lock (closeLocker)
            {

                if (readEventArgs == null || Socket == null || !Connected || (Socket != null && !Socket.Connected))
                    return;

                AsyncUserToken token = e.UserToken as AsyncUserToken;

                if (token != null)
                {
                    try
                    {
                        bool conn = Connected;

                        Interlocked.CompareExchange(ref connected, Convert.ToInt32(false), Convert.ToInt32(true));

                        token.Socket.Shutdown(SocketShutdown.Both);

                        if (ClientDisconnected != null && conn)
                            ClientDisconnected(this, new ClientDisconnectedEventArgs(this));

                        if (InternalClientDisconnected != null && conn)
                            InternalClientDisconnected(this, new ClientDisconnectedEventArgs(this));
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        if (token.Socket != null)
                            token.Socket.Close();
                    }
                }
            }
        }

        public void Disconnect()
        {
            CloseClientSocket(readEventArgs);
        }

        #region Commands

        public Task WarpTo(string name)
        {
            return Server.ServerClient.SendPacket(new WarpCommandPacket { Player = name, WarpType = WarpType.WarpOtherShip });
        }

        public Task WarpTo(StarboundPlayer player)
        {
            return WarpTo(player.NameWithColor);
        }

        public Task WarpUp()
        {
            return Server.ServerClient.SendPacket(new WarpCommandPacket { WarpType = WarpType.WarpUp });
        }

        public Task WarpDown()
        {
            return Server.ServerClient.SendPacket(new WarpCommandPacket { WarpType = WarpType.WarpDown });
        }

        public Task MoveShip(WorldCoordinate coordinates)
        {
            return Server.ServerClient.SendPacket(new WarpCommandPacket
            {
                WarpType = WarpType.MoveShip,
                Coordinates = coordinates
            });
        }

        public void SendChatMessage(string name, string message)
        {
            SendChatMessage(message, 0, String.Empty, name);
        }

        public Task SendChatMessage(string message, int channel, string world, string name)
        {
            return Server.PlayerClient.SendPacket(new ChatReceivedPacket
            {
                Message = message,
                Channel = (byte)channel,
                ClientId = 0,
                Name = name,
                World = world
            });
        }

        #endregion


        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (readEventArgs != null)
                    readEventArgs.Dispose();

                if (heartbeatChecker != null)
                    heartbeatChecker.Dispose();
            }

            heartbeatChecker = null;
            Socket = null;
            readEventArgs = null;

        }

        ~SharpStarClient()
        {
            Dispose(false);
        }

    }

    public class PacketEventArgs : EventArgs
    {

        public SharpStarClient Client { get; set; }

        public IPacket Packet { get; set; }

        public PacketEventArgs(SharpStarClient client, IPacket packet)
        {
            Client = client;
            Packet = packet;
        }

    }

}
