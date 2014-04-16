using System;
using System.Net;
using System.Net.Sockets;
using SharpStar.Lib.Entities;
using System.Threading;

namespace SharpStar.Lib.Server
{
    public class StarboundServerClient : IDisposable
    {

        public const string Host = "127.0.0.1";

        public bool Connected { get; private set; }

        public TcpClient ServerTcpClient { get; set; }


        public event EventHandler Disconnected;


        public StarboundClient ServerClient { get; set; }

        public StarboundClient PlayerClient { get; set; }

        public StarboundPlayer Player { get; set; }

        public DateTime ConnectionTime { get; private set; }


        public event EventHandler SClientConnected;

        private volatile bool _disconnectEventCalled;

        private readonly object _locker = new object();


        public StarboundServerClient(StarboundClient plrClient)
        {

            _disconnectEventCalled = false;

            PlayerClient = plrClient;
            PlayerClient.Server = this;

            ServerTcpClient = new TcpClient();
            ServerTcpClient.NoDelay = true;

            ServerClient = new StarboundClient(ServerTcpClient.Client, Direction.Server);
            ServerClient.OtherClient = PlayerClient;
            ServerClient.Server = this;

            PlayerClient.OtherClient = ServerClient;

            Connected = false;

        }

        public void Connect(int port)
        {

            ServerTcpClient.BeginConnect(Host, port, ServerClientConnected, null);

            ConnectionTime = DateTime.Now;

        }

        private void ServerClientConnected(IAsyncResult iar)
        {
            try
            {
                ServerTcpClient.EndConnect(iar);

                Connected = true;

                PlayerClient.StartReceiving();
                ServerClient.StartReceiving();
            }
            catch (Exception)
            {
                ForceDisconnect();
            }
        }

        public void ForceDisconnect()
        {

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

                ForceDisconnect();

                if (PlayerClient != null)
                    PlayerClient.Dispose();

                if (ServerClient != null)
                    ServerClient.Dispose();
            }

            PlayerClient = null;
            ServerClient = null;
        }

        #endregion
    }

}