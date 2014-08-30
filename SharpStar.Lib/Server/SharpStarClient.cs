using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
            this.readEventArgs = eventArgs;
            this.readEventArgs.Completed += IO_Completed;
            this.Direction = dir;
            PacketReader = new PacketReader();
            PacketQueue = new ConcurrentQueue<IPacket>();
        }

        public void StartReceive()
        {
            connected = Convert.ToInt32(true);

            AsyncUserToken token = (AsyncUserToken)readEventArgs.UserToken;

            Socket = token.Socket;
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
                ProcessReceive(readEventArgs);
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {

                PacketReader.NetworkBuffer = new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred);

                foreach (IPacket packet in PacketReader.UpdateBuffer(true))
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
                                handler.Handle(packet, this);
                        }

                        SharpStarMain.Instance.PluginManager.CallEvent(packet, OtherClient);

                        if (!packet.Ignore && OtherClient != null)
                            OtherClient.SendPacket(packet);

                        foreach (IPacketHandler handler in handlers)
                        {
                            if (packet.PacketId == handler.PacketId)
                                handler.HandleAfter(packet, this);
                        }

                        EventHandler<PacketEventArgs> afterPacketArgs = AfterPacketReceived;
                        if (afterPacketArgs != null)
                            afterPacketArgs(this, new PacketEventArgs(this, packet));

                        SharpStarMain.Instance.PluginManager.CallEvent(packet, OtherClient, true);
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
                        ProcessReceive(e);
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

        public void SendPacket(IPacket packet)
        {

            EventHandler<PacketEventArgs> sendingPacket = SendingPacket;
            if (sendingPacket != null)
                sendingPacket(this, new PacketEventArgs(this, packet));

            PacketQueue.Enqueue(packet);
            FlushPackets();
        }

        public void RegisterPacketHandler(IPacketHandler packetHandler)
        {
            throw new NotImplementedException();
        }

        public void UnregisterPacketHandler(IPacketHandler handler)
        {
            throw new NotImplementedException();
        }

        public void ForceDisconnect()
        {
            this.CloseClientSocket(readEventArgs);
        }

        public void FlushPackets()
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
                                handler.Handle(next, OtherClient);
                        }

                    }

                    if (next.Ignore)
                        continue;

                    var stream = new StarboundStream();
                    next.Write(stream);
                    byte[] buffer = stream.ToArray();

                    stream.Dispose();

                    int length = buffer.Length;

                    if (length >= 1024)
                    {
                        byte[] compressed = ZlibStream.CompressBuffer(buffer);

                        if (compressed.Length < buffer.Length)
                        {
                            buffer = compressed;
                            length = -buffer.Length;
                        }
                    }

                    var finalStream = new StarboundStream();

                    finalStream.WriteUInt8(next.PacketId);
                    finalStream.WriteSignedVLQ(length);
                    finalStream.Write(buffer, 0, buffer.Length);

                    byte[] toSend = finalStream.ToArray();

                    finalStream.Dispose();

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

                if (readEventArgs == null || Socket == null || !Connected || (Socket != null && !Socket.Connected) )
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

        public void WarpTo(string name)
        {
            Server.ServerClient.SendPacket(new WarpCommandPacket { Player = name, WarpType = WarpType.WarpOtherShip });
        }

        public void WarpTo(StarboundPlayer player)
        {
            WarpTo(player.NameWithColor);
        }

        public void WarpUp()
        {
            Server.ServerClient.SendPacket(new WarpCommandPacket { WarpType = WarpType.WarpUp });
        }

        public void WarpDown()
        {
            Server.ServerClient.SendPacket(new WarpCommandPacket { WarpType = WarpType.WarpDown });
        }

        public void MoveShip(WorldCoordinate coordinates)
        {
            Server.ServerClient.SendPacket(new WarpCommandPacket
            {
                WarpType = WarpType.MoveShip,
                Coordinates = coordinates
            });
        }

        public void SendChatMessage(string name, string message)
        {
            SendChatMessage(message, 0, String.Empty, name);
        }

        public void SendChatMessage(string message, int channel, string world, string name)
        {
            Server.PlayerClient.SendPacket(new ChatReceivedPacket
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
