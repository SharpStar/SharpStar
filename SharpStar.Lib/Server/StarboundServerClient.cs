using System;
using System.Net;
using System.Net.Sockets;
using SharpStar.Lib.Entities;
using System.Threading;

namespace SharpStar.Lib.Server
{
    public class StarboundServerClient : IDisposable
    {

        public static BufferManager BufferManager;

        public Socket ServerClientSocket { get; set; }

        private volatile bool _disconnectEventCalled;

        private readonly object _locker = new object();


        public event EventHandler Disconnected;


        public const string Host = "127.0.0.1";

        public StarboundClient ServerClient { get; set; }

        public StarboundClient PlayerClient { get; set; }

        public bool Connected { get; private set; }

        public StarboundPlayer Player { get; set; }

        public DateTime ConnectionTime { get; private set; }


        public event EventHandler ServerClientConnected;

        private SocketAsyncEventArgs _clientArgs;

        public StarboundServerClient(StarboundClient plrClient, SocketAsyncEventArgs clientArgs)
        {

            _disconnectEventCalled = false;

            PlayerClient = plrClient;
            PlayerClient.Server = this;

            _clientArgs = clientArgs;
            _clientArgs.Completed += OnCompleted;

            Connected = false;

        }

        public void Connect(int port)
        {

            Token token = _clientArgs.UserToken as Token;
            Socket clientSock = token.Connection;

            if (!clientSock.ReceiveAsync(_clientArgs))
            {
                this.ProcessReceive(_clientArgs);
            }

            PlayerClient.Socket = clientSock;

            var ipe = new IPEndPoint(IPAddress.Parse(Host), port);
            this.ServerClientSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.ServerClientSocket.NoDelay = true;

            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();

            connectArgs.RemoteEndPoint = ipe;
            connectArgs.Completed += OnCompleted;

            ServerClient = new StarboundClient(ServerClientSocket, Direction.Server);
            ServerClient.OtherClient = PlayerClient;
            ServerClient.Server = this;

            PlayerClient.OtherClient = ServerClient;

            this.ServerClientSocket.ConnectAsync(connectArgs);

            ConnectionTime = DateTime.Now;

        }

        private void OnCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    this.ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    this.ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {

            SocketError errorCode = e.SocketError;

            if (errorCode != SocketError.Success)
            {

                ForceDisconnect();

                return;

            }

            this.Connected = (e.SocketError == SocketError.Success);


            if (ServerClientConnected != null)
                ServerClientConnected(this, EventArgs.Empty);


            e.UserToken = new Token(ServerClientSocket, ServerClient);

            BufferManager.SetBuffer(e);

            if (!this.ServerClientSocket.ReceiveAsync(e))
            {
                this.ProcessReceive(e);
            }

        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {

            Token token = e.UserToken as Token;
            Socket s = token.Connection;

            try
            {

                if (e.BytesTransferred > 0)
                {

                    if (e.SocketError == SocketError.Success)
                    {

                        token.SetData(e);
                        token.ProcessData(e);

                        if (!s.ReceiveAsync(e))
                        {
                            this.ProcessReceive(e);
                        }

                    }
                    else
                    {
                        this.ProcessError(e);
                    }

                }
                else
                {
                    ForceDisconnect();
                }

            }
            catch (Exception)
            {
                ForceDisconnect();
            }

        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {

            if (e.SocketError == SocketError.Success)
            {
                Token token = e.UserToken as Token;

                if (!token.Connection.ReceiveAsync(e))
                {
                    this.ProcessReceive(e);
                }
            }
            else
            {
                this.ProcessError(e);
            }

        }


        private void ProcessError(SocketAsyncEventArgs e)
        {

            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                }
                finally
                {
                    ForceDisconnect();
                }
            }

            throw new SocketException((int)e.SocketError);

        }

        public void ForceDisconnect()
        {

            Connected = false;

            lock (_locker)
            {

                if (Disconnected != null && !_disconnectEventCalled)
                    Disconnected(this, EventArgs.Empty);

                _disconnectEventCalled = true;

            }

            if (this.ServerClientSocket != null && this.ServerClientSocket.Connected)
            {
                this.ServerClientSocket.Disconnect(false);
                this.ServerClientSocket.Dispose();
            }

            if (PlayerClient != null && PlayerClient.Socket != null && PlayerClient.Socket.Connected)
            {
                this.PlayerClient.Socket.Disconnect(false);
                this.PlayerClient.Socket.Dispose();
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