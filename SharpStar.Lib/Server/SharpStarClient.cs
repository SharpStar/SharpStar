using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zlib;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Extensions;
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

        public event EventHandler<PacketEventArgs> SendingPacket;

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        public ConcurrentQueue<IPacket> PacketQueue { get; set; }

        private int connected;

        public bool Connected
        {
            get
            {
                return Convert.ToBoolean(connected);
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
                List<IPacket> packets = PacketReader.UpdateBuffer(true);

                foreach (IPacket packet in packets)
                {

                    try
                    {
                        if (PacketReceived != null)
                            PacketReceived(this, new PacketEventArgs(this, packet));

                        foreach (IPacketHandler handler in Server.PacketHandlers)
                        {
                            if (packet.PacketId == handler.PacketId)
                                handler.Handle(packet, this);
                        }

                        SharpStarMain.Instance.PluginManager.CallEvent(packet, OtherClient);

                        if (!packet.Ignore)
                            OtherClient.SendPacket(packet);

                        foreach (IPacketHandler handler in Server.PacketHandlers)
                        {
                            if (packet.PacketId == handler.PacketId)
                                handler.HandleAfter(packet, this);
                        }

                        SharpStarMain.Instance.PluginManager.CallEvent(packet, OtherClient, true);

                        if (packet is DisconnectResponsePacket)
                        {
                            this.CloseClientSocket(e);
                        }

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
            if (SendingPacket != null)
                SendingPacket(this, new PacketEventArgs(this, packet));

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

                    var memoryStream = new MemoryStream();

                    var stream = new StarboundStream(memoryStream);
                    next.Write(stream);
                    byte[] buffer = memoryStream.ToArray();

                    memoryStream.Dispose();

                    int length = buffer.Length;
                    byte[] compressed = ZlibStream.CompressBuffer(buffer);

                    if (compressed.Length < buffer.Length)
                    {
                        buffer = compressed;
                        length = -buffer.Length;
                    }

                    var finalMemStream = new MemoryStream();
                    var finalStream = new StarboundStream(finalMemStream);

                    finalStream.WriteUInt8(next.PacketId);
                    finalStream.WriteSignedVLQ(length);
                    finalStream.Write(buffer, 0, buffer.Length);

                    byte[] toSend = finalMemStream.ToArray();

                    stream.Dispose();
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {

            if (readEventArgs == null || Socket == null || (Socket != null && !Socket.Connected) || !Connected)
                return;

            AsyncUserToken token = e.UserToken as AsyncUserToken;

            if (token != null)
            {
                try
                {
                    token.Socket.Shutdown(SocketShutdown.Send);

                    if (ClientDisconnected != null && Connected)
                        ClientDisconnected(this, new ClientDisconnectedEventArgs(this));
                }
                catch (Exception)
                {
                }
                finally
                {
                    Interlocked.CompareExchange(ref connected, Convert.ToInt32(false), Convert.ToInt32(true));
                }

                token.Socket.Close();
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
            WarpTo(player.Name);
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
                readEventArgs.Dispose();
            }

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
