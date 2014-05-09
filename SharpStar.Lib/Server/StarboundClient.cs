using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using Ionic.Zlib;
using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Entities;
using SharpStar.Lib.Extensions;
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


        private readonly List<IPacketHandler> _packetHandlers;

        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        private readonly Timer _connTimer;

        public StarboundClient(Socket socket, Direction dir)
        {

            Socket = socket;
            Direction = dir;

            _packetHandlers = new List<IPacketHandler>();

            PacketQueue = new ConcurrentQueue<IPacket>();

            PacketReader = new PacketReader();
            PacketReader.RegisterPacketType(0, typeof(ProtocolVersionPacket));
            PacketReader.RegisterPacketType(1, typeof(ConnectionResponsePacket));
            PacketReader.RegisterPacketType(2, typeof(DisconnectResponsePacket));
            PacketReader.RegisterPacketType(3, typeof(HandshakeChallengePacket));
            PacketReader.RegisterPacketType(4, typeof(ChatReceivedPacket));
            PacketReader.RegisterPacketType(5, typeof(UniverseTimeUpdatePacket));
            PacketReader.RegisterPacketType(7, typeof(ClientConnectPacket));
            PacketReader.RegisterPacketType(8, typeof(ClientDisconnectPacket));
            PacketReader.RegisterPacketType(9, typeof(HandshakeResponsePacket));
            PacketReader.RegisterPacketType(10, typeof(WarpCommandPacket));
            PacketReader.RegisterPacketType(11, typeof(ChatSentPacket));
            PacketReader.RegisterPacketType(13, typeof(ClientContextUpdatePacket));
            PacketReader.RegisterPacketType(14, typeof(WorldStartPacket));
            PacketReader.RegisterPacketType(15, typeof(WorldStopPacket));
            PacketReader.RegisterPacketType(19, typeof(TileDamageUpdatePacket));
            PacketReader.RegisterPacketType(21, typeof(GiveItemPacket));
            PacketReader.RegisterPacketType(23, typeof(EnvironmentUpdatePacket));
            PacketReader.RegisterPacketType(24, typeof(EntityInteractResultPacket));
            PacketReader.RegisterPacketType(28, typeof(RequestDropPacket));
            PacketReader.RegisterPacketType(33, typeof(OpenContainerPacket));
            PacketReader.RegisterPacketType(34, typeof(CloseContainerPacket));
            PacketReader.RegisterPacketType(42, typeof(EntityCreatePacket));
            //PacketReader.RegisterPacketType(43, typeof(EntityUpdatePacket));
            PacketReader.RegisterPacketType(44, typeof(EntityDestroyPacket));
            PacketReader.RegisterPacketType(45, typeof(DamageNotificationPacket));
            PacketReader.RegisterPacketType(47, typeof(UpdateWorldPropertiesPacket));
            PacketReader.RegisterPacketType(48, typeof(HeartbeatPacket));

            RegisterPacketHandler(new UnknownPacketHandler());
            RegisterPacketHandler(new ProtocolVersionPacketHandler());
            RegisterPacketHandler(new ClientConnectPacketHandler());
            RegisterPacketHandler(new ClientDisconnectPacketHandler());
            RegisterPacketHandler(new HandshakeResponsePacketHandler());
            RegisterPacketHandler(new ChatSentPacketHandler());
            RegisterPacketHandler(new RequestDropPacketHandler());
            RegisterPacketHandler(new WarpCommandPacketHandler());
            RegisterPacketHandler(new OpenContainerPacketHandler());
            RegisterPacketHandler(new CloseContainerPacketHandler());
            RegisterPacketHandler(new DamageNotificationPacketHandler());
            RegisterPacketHandler(new ConnectionResponsePacketHandler());
            RegisterPacketHandler(new HandshakeChallengePacketHandler());
            RegisterPacketHandler(new DisconnectResponsePacketHandler());
            RegisterPacketHandler(new ChatReceivedPacketHandler());
            RegisterPacketHandler(new EntityInteractResultPacketHandler());
            RegisterPacketHandler(new UniverseTimeUpdatePacketHandler());
            RegisterPacketHandler(new ClientContextUpdatePacketHandler());
            RegisterPacketHandler(new WorldStartPacketHandler());
            RegisterPacketHandler(new TileDamageUpdatePacketHandler());
            RegisterPacketHandler(new GiveItemPacketHandler());
            RegisterPacketHandler(new EnvironmentUpdatePacketHandler());
            RegisterPacketHandler(new EntityCreatePacketHandler());
            //RegisterPacketHandler(new EntityUpdatePacketHandler());
            RegisterPacketHandler(new EntityDestroyPacketHandler());
            RegisterPacketHandler(new UpdateWorldPropertiesPacketHandler());
            RegisterPacketHandler(new HeartbeatPacketHandler());

            _connTimer = new Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);
            _connTimer.Elapsed += (sender, e) => CheckConnection();
            _connTimer.Start();

        }

        public void RegisterPacketHandler(IPacketHandler handler)
        {
            _packetHandlers.Add(handler);
        }

        public void UnregisterPacketHandler(IPacketHandler handler)
        {
            _packetHandlers.Remove(handler);
        }

        #region Connection

        public void StartReceiving()
        {
            Socket.BeginReceive(PacketReader.NetworkBuffer, 0, PacketReader.NetworkBuffer.Length, SocketFlags.None,
                ClientDataReceived, null);
        }

        public void ForceDisconnect()
        {
            Server.ForceDisconnect();
        }

        public void SendPacket(IPacket packet)
        {
            PacketQueue.Enqueue(packet);
            FlushPackets();
        }

        private void ClientDataReceived(IAsyncResult iar)
        {

            if (Socket == null)
                return;

            try
            {

                int length = Socket.EndReceive(iar);

                List<IPacket> packets = PacketReader.UpdateBuffer(null, length);

                foreach (var packet in packets)
                {

                    if (PacketReceived != null)
                        PacketReceived(this, new PacketReceivedEventArgs(packet));


                    SharpStarMain.Instance.PluginManager.CallEvent("packetReceived", packet, OtherClient);

                    foreach (var handler in _packetHandlers)
                    {
                        if (packet.PacketId == handler.PacketId)
                            handler.Handle(packet, this);
                    }

                    if (!packet.Ignore)
                        OtherClient.SendPacket(packet);

                    SharpStarMain.Instance.PluginManager.CallEvent("afterPacketReceived", packet, OtherClient);

                    foreach (var handler in _packetHandlers)
                    {
                        if (packet.PacketId == handler.PacketId)
                            handler.HandleAfter(packet, this);
                    }

                }

                Socket.BeginReceive(PacketReader.NetworkBuffer, 0, PacketReader.NetworkBuffer.Length, SocketFlags.None, ClientDataReceived, null);

            }
            catch (EndOfStreamException)
            {
                Socket.BeginReceive(PacketReader.NetworkBuffer, 0, PacketReader.NetworkBuffer.Length, SocketFlags.None, ClientDataReceived, null);
            }
            catch (Exception)
            {
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

            if (!Server.Connected)
                return;

            while (PacketQueue.Count > 0)
            {

                try
                {
                    IPacket next;

                    while (!PacketQueue.TryDequeue(out next))
                    {
                    }

                    if (!next.IsReceive)
                    {

                        SharpStarMain.Instance.PluginManager.CallEvent("customPacketSending", next, OtherClient);

                        foreach (var handler in _packetHandlers)
                        {
                            if (next.PacketId == handler.PacketId)
                                handler.Handle(next, OtherClient);
                        }

                        if (next.Ignore)
                            continue;

                    }

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

        public bool CheckConnection()
        {

            if (Socket == null || (Socket != null && !Socket.IsConnected()))
            {
                ForceDisconnect();

                return false;
            }

            return true;

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

                if (_connTimer != null)
                {
                    _connTimer.Stop();
                    _connTimer.Close();
                }

            }

            Socket = null;

        }

        #endregion
    }

    public class PacketReceivedEventArgs : EventArgs
    {

        public IPacket Packet { get; set; }

        public PacketReceivedEventArgs(IPacket packet)
        {
            Packet = packet;
        }

    }

}