using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharpStar.Lib.Server
{

    /// <summary>
    /// Token for use with SocketAsyncEventArgs.
    /// </summary>
    internal sealed class Token : IDisposable
    {

        public StarboundClient SClient { get; set; }

        private readonly Socket connection;

        private int currentIndex;

        internal Token(Socket connection, StarboundClient sClient)
        {
            this.connection = connection;
            this.SClient = sClient;
        }

        /// <summary>
        /// Accept socket.
        /// </summary>
        internal Socket Connection
        {
            get { return this.connection; }
        }

        /// <summary>
        /// Process data received from the client.
        /// </summary>
        /// <param name="args">SocketAsyncEventArgs used in the operation.</param>
        internal void ProcessData(SocketAsyncEventArgs args)
        {

            SClient.IncomingData(SClient.PacketReader.NetworkBuffer.Length);

            this.currentIndex = 0;
        }

        /// <summary>
        /// Set data received from the client.
        /// </summary>
        /// <param name="args">SocketAsyncEventArgs used in the operation.</param>
        internal void SetData(SocketAsyncEventArgs args)
        {

            int count = args.BytesTransferred;

            SClient.PacketReader.NetworkBuffer = new byte[count];

            Buffer.BlockCopy(args.Buffer, args.Offset, SClient.PacketReader.NetworkBuffer, 0, SClient.PacketReader.NetworkBuffer.Length);


            this.currentIndex += count;

        }

        #region IDisposable Members

        /// <summary>
        /// Release instance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.connection.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
                // Throw if client has closed, so it is not necessary to catch.
            }
            finally
            {
                this.connection.Close();
            }
        }

        #endregion
    }
}
