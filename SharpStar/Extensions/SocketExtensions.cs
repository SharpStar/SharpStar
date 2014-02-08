using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SharpStar.Extensions
{
    public static class SocketExtensions
    {

        public static bool IsConnected(this Socket socket)
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && (socket.Available == 0) || !socket.Connected);
        }

    }
}
