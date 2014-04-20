using System;
using System.Net.Sockets;

namespace SharpStar.Lib.Extensions
{
    public static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && (socket.Available == 0) || !socket.Connected);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}