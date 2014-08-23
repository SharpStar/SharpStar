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

        private int running;

        public bool Running
        {
            get
            {
                return Convert.ToBoolean(running);
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

            Task.Run(async () =>
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
            });

        }

        public void Stop()
        {
            Interlocked.CompareExchange(ref running, Convert.ToInt32(false), Convert.ToInt32(true));
        }
    }
}
