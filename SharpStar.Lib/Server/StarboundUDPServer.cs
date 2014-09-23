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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpStar.Lib.Extensions;

namespace SharpStar.Lib.Server
{
    public class StarboundUDPServer : IServer
    {

        private readonly UdpClient udpServer;
        private readonly UdpClient udpClient;

        private readonly IPAddress sharpStarBind;
        private readonly IPAddress starboundBind;

        private readonly int serverPort;
        private readonly int listenPort;

        private IPEndPoint sIpe;
        private IPEndPoint cIpe;

        private long running;

        public bool Running
        {
            get
            {
                return Convert.ToBoolean(Interlocked.Read(ref running));
            }
        }

        public StarboundUDPServer()
        {
            running = Convert.ToInt32(false);

            string shBind = SharpStarMain.Instance.Config.ConfigFile.SharpStarBind;

            if (shBind == "*")
                sharpStarBind = IPAddress.Any;
            else
                sharpStarBind = IPAddress.Parse(shBind);

            string sbBind = SharpStarMain.Instance.Config.ConfigFile.StarboundBind;

            if (string.IsNullOrEmpty(sbBind))
                starboundBind = IPAddress.Parse("127.0.0.1");
            else
                starboundBind = IPAddress.Parse(sbBind);

            serverPort = SharpStarMain.Instance.Config.ConfigFile.ServerPort;
            listenPort = SharpStarMain.Instance.Config.ConfigFile.ListenPort;

            udpClient = new UdpClient();
            udpServer = new UdpClient(listenPort);
        }

        public void Start()
        {
            Stop();

            Interlocked.CompareExchange(ref running, Convert.ToInt32(true), Convert.ToInt32(false));

            sIpe = new IPEndPoint(sharpStarBind, listenPort);
            cIpe = new IPEndPoint(starboundBind, serverPort);

            udpClient.Connect(cIpe);

            Task.Run(() => RunUdp().Wait()).ContinueWith(_ => {}, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task RunUdp()
        {
            while (Running)
            {
                try
                {
                    UdpReceiveResult result = await udpServer.ReceiveAsync();

                    byte[] buffer = result.Buffer;

                    await udpClient.SendAsync(buffer, buffer.Length);

                    UdpReceiveResult result2 = await udpClient.ReceiveAsync();

                    byte[] buffer2 = result2.Buffer;

                    await udpServer.SendAsync(buffer2, buffer2.Length);
                }
                catch
                {
                }
            }
        }

        public void Stop()
        {
            Interlocked.CompareExchange(ref running, Convert.ToInt32(false), Convert.ToInt32(true));
        }
    }
}
