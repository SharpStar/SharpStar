using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using SharpStar.Entities;
using SharpStar.Packets;
using SharpStar.Packets.Handlers;

namespace SharpStar.Server
{
    public class StarboundServerClient : IDisposable
    {

        public event EventHandler Disconnected;

        public const string Host = "127.0.0.1";

        public TcpClient ServerTcpClient { get; set; }

        public StarboundClient ServerClient { get; set; }

        public StarboundClient PlayerClient { get; set; }

        public bool Connected { get; private set; }

        public StarboundPlayer Player { get; set; }

        public StarboundServerClient(StarboundClient plrClient)
        {

            PlayerClient = plrClient;
            PlayerClient.Server = this;
            PlayerClient.StartReceiving();

            ServerTcpClient = new TcpClient();

            ServerClient = new StarboundClient(ServerTcpClient.Client);
            ServerClient.OtherClient = PlayerClient;
            ServerClient.Server = this;

            PlayerClient.OtherClient = ServerClient;

            Connected = false;

        }

        public void Connect(int port)
        {
            ServerTcpClient.BeginConnect(Host, port, ServerClientConnected, null);
        }

        public void ForceDisconnect()
        {

            if (!Connected)
                return;

            Connected = false;

            try
            {

                if (ServerTcpClient != null)
                    ServerTcpClient.Close();

                if (ServerClient != null)
                {

                    if (ServerClient.Socket != null)
                    {
                        ServerClient.Socket.Close();
                    }

                    ServerClient.Dispose();

                }

                if (PlayerClient != null)
                {

                    if (PlayerClient.Socket != null)
                    {
                        PlayerClient.Socket.Close();
                    }

                    PlayerClient.Dispose();

                }

            }
            catch (Exception)
            {
            }
            finally
            {

                if (Disconnected != null)
                    Disconnected(this, EventArgs.Empty);

            }

        }

        private void ServerClientConnected(IAsyncResult iar)
        {

            try
            {

                ServerTcpClient.EndConnect(iar);

                Connected = true;

                ServerClient.StartReceiving();

            }
            catch (Exception)
            {
                ForceDisconnect();
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

                if (ServerTcpClient != null)
                    ServerTcpClient.Close();

                if (PlayerClient != null)
                    PlayerClient.Dispose();

                if (ServerClient != null)
                    ServerClient.Dispose();

            }

            ServerTcpClient = null;
            PlayerClient = null;
            ServerClient = null;

        }

        #endregion

    }
}
