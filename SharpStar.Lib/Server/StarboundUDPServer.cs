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

        private bool running;

        public StarboundUDPServer()
        {
            running = false;

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

            running = true;

            sIpe = new IPEndPoint(sharpStarBind, listenPort);
            cIpe = new IPEndPoint(starboundBind, serverPort);

            udpClient.Connect(cIpe);

            try
            {
                udpServer.BeginReceive(Receive, null);
            }
            catch (Exception ex)
            {
                ex.LogError();
            }

        }

        private void Receive(IAsyncResult iar)
        {
            try
            {
                byte[] data = udpServer.EndReceive(iar, ref sIpe);

                udpClient.BeginSend(data, data.Length, Send, null);
            }
            catch
            {
                if (running)
                {
                    udpServer.BeginReceive(Receive, udpClient);
                }
            }
        }

        private void Send(IAsyncResult iar)
        {
            try
            {
                udpClient.EndSend(iar);

                udpClient.BeginReceive(Receive2, null);

            }
            catch
            {
                if (running)
                {
                    udpServer.BeginReceive(Receive, udpClient);
                }
            }
        }

        private void Receive2(IAsyncResult iar)
        {
            try
            {
                byte[] data = udpClient.EndReceive(iar, ref cIpe);

                udpServer.BeginSend(data, data.Length, sIpe, Send2, null);
            }
            catch
            {
                if (running)
                {
                    udpServer.BeginReceive(Receive, udpClient);
                }
            }
        }

        private void Send2(IAsyncResult iar)
        {
            try
            {
                udpClient.EndSend(iar);
            }
            catch
            {
            }
            finally
            {
                try
                {
                    if (running)
                    {
                        udpServer.BeginReceive(Receive, null);
                    }
                }
                catch
                {
                }
            }
        }

        public void Stop()
        {
            running = false;
        }
    }
}
