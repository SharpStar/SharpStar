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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;
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
    public class StarboundClient : IClient, IDisposable
    {
        public Socket Socket { get; set; }

        public string IPAddress
        {
            get
            {
                if (Socket == null)
                    return String.Empty;

                return ((IPEndPoint)Socket.RemoteEndPoint).Address.ToString();
            }
        }

        public ConcurrentQueue<IPacket> PacketQueue { get; set; }

        public StarboundServerClient Server { get; set; }

        public StarboundClient OtherClient { get; set; }

        public PacketReader PacketReader { get; set; }

        public Direction Direction { get; private set; }


        public List<IPacketHandler> PacketHandlers { get; private set; }

        public event EventHandler<PacketEventArgs> PacketReceived;

        public event EventHandler<PacketEventArgs> SendingPacket;

        private readonly object _handlerLock = new object();

        private Timer _connectionTimer;

        public StarboundClient(Socket socket, Direction dir)
        {
            _connectionTimer = new Timer();
            _connectionTimer.Interval = TimeSpan.FromSeconds(15).TotalMilliseconds;
            _connectionTimer.Elapsed += (s, e) =>
            {
                if (Socket != null && !Socket.IsConnected())
                    ForceDisconnect();
            };

            Socket = socket;
            Direction = dir;

            PacketHandlers = new List<IPacketHandler>();

            PacketQueue = new ConcurrentQueue<IPacket>();
            PacketReader = new PacketReader();
        }

        public void RegisterPacketHandler(IPacketHandler handler)
        {
            if (PacketHandlers.All(p => p.GetType() != handler.GetType()))
                PacketHandlers.Add(handler);
        }

        public void UnregisterPacketHandler(IPacketHandler handler)
        {
            PacketHandlers.Remove(handler);
        }

        public void ClearPacketHandlers()
        {
            lock (_handlerLock)
            {
                PacketHandlers.Clear();
            }
        }

        #region Connection

        public void StartReceiving()
        {
            if (Socket != null && PacketReader != null)
            {
                Socket.SetSocketKeepAliveValues(1000, 100);

                _connectionTimer.Start();

                try
                {
                    Socket.BeginReceive(PacketReader.NetworkBuffer, 0, PacketReader.NetworkBuffer.Length, SocketFlags.None,
                        ClientDataReceived, Socket);
                }
                catch
                {
                    ForceDisconnect();
                }
            }
            else
            {
                ForceDisconnect();
            }
        }

        public void ForceDisconnect()
        {

            if (Socket == null)
                return;

            try
            {
                if (Socket != null)
                {
                    Socket.Disconnect(true);
                    Socket.Dispose();
                }

                if (PacketReader != null)
                {
                    PacketReader.Dispose();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                PacketReader = null;
                Socket = null;
            }

            if (Server != null)
                Server.ForceDisconnect();

            if (OtherClient != null && OtherClient.Socket != null)
                OtherClient.ForceDisconnect();

        }

        public void SendPacket(IPacket packet)
        {
            if (SendingPacket != null)
                SendingPacket(this, new PacketEventArgs(this, packet));

            PacketQueue.Enqueue(packet);
            FlushPackets();
        }

        private void ClientDataReceived(IAsyncResult iar)
        {

            Socket sock = (Socket)iar.AsyncState;

            if (sock == null)
            {
                ForceDisconnect();

                return;
            }

            try
            {

                int length = sock.EndReceive(iar);

                List<IPacket> packets = PacketReader.UpdateBuffer(null, length);

                foreach (var packet in packets)
                {

                    if (PacketReceived != null)
                        PacketReceived(this, new PacketEventArgs(this, packet));


                    SharpStarMain.Instance.PluginManager.CallEvent("packetReceived", packet, OtherClient);


                    foreach (var handler in PacketHandlers.ToList())
                    {
                        if (packet.PacketId == handler.PacketId)
                            handler.Handle(packet, this);
                    }

                    if (!packet.Ignore)
                        OtherClient.SendPacket(packet);

                    SharpStarMain.Instance.PluginManager.CallEvent("afterPacketReceived", packet, OtherClient);


                    foreach (var handler in PacketHandlers.ToList())
                    {
                        if (packet.PacketId == handler.PacketId)
                            handler.HandleAfter(packet, this);
                    }

                    if (packet is DisconnectResponsePacket)
                    {
                        ForceDisconnect();
                    }

                }

                if (PacketReader != null && PacketReader.NetworkBuffer != null && sock.Connected)
                    sock.BeginReceive(PacketReader.NetworkBuffer, 0, PacketReader.NetworkBuffer.Length, SocketFlags.None, ClientDataReceived, sock);

            }
            catch (EndOfStreamException ex)
            {
                SharpStarLogger.DefaultLogger.Error(ex.ToString());
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                if (PacketReader != null && PacketReader.NetworkBuffer != null && Socket != null && Socket.Connected)
                    sock.BeginReceive(PacketReader.NetworkBuffer, 0, PacketReader.NetworkBuffer.Length, SocketFlags.None, ClientDataReceived, sock);
                else
                    ForceDisconnect();
            }

        }

        private void PacketSent(IAsyncResult iar)
        {
            try
            {
                Socket.EndSend(iar);
            }
            catch (Exception)
            {
                ForceDisconnect();
            }
        }

        public void FlushPackets()
        {

            while (PacketQueue.Count > 0)
            {

                if (Socket == null || !Socket.Connected || OtherClient == null)
                    return;

                try
                {
                    IPacket next;

                    while (!PacketQueue.TryDequeue(out next))
                    {
                    }

                    if (!next.IsReceive)
                    {

                        SharpStarMain.Instance.PluginManager.CallEvent("customPacketSending", next, OtherClient);

                        foreach (var handler in PacketHandlers.ToList())
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

                    int length = buffer.Length;
                    var compressed = ZlibStream.CompressBuffer(buffer);
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

                    Socket.BeginSend(toSend, 0, toSend.Length, SocketFlags.None, PacketSent, null);

                    stream.Close();
                    finalStream.Close();

                    stream.Dispose();
                    finalStream.Dispose();

                }
                catch (Exception)
                {
                    ForceDisconnect();
                }
            }
        }

        #endregion

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

        public void Disconnect()
        {
            try
            {
                Server.ServerClient.SendPacket(new ClientDisconnectPacket());
            }
            catch (Exception)
            {
                ForceDisconnect();
            }
        }

        #endregion

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
                if (PacketReader != null)
                    PacketReader.Dispose();

                if (Socket != null)
                    Socket.Dispose();

                if (_connectionTimer != null)
                {
                    _connectionTimer.Stop();
                    _connectionTimer.Dispose();
                    _connectionTimer = null;
                }
            }

            Socket = null;
        }

        ~StarboundClient()
        {
            Dispose(false);
        }

        #endregion
    }

    public class PacketEventArgs : EventArgs
    {

        public StarboundClient Client { get; set; }

        public IPacket Packet { get; set; }

        public PacketEventArgs(StarboundClient client, IPacket packet)
        {
            Client = client;
            Packet = packet;
        }

    }

}