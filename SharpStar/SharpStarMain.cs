using System;
using System.IO;
using System.Reflection;
using SharpStar.Config;
using SharpStar.Plugins;
using SharpStar.Server;

namespace SharpStar
{
    public class SharpStarMain
    {

        private static readonly object SyncRoot = new object();

        private static SharpStarMain _instance;

        public static SharpStarMain Instance
        {
            get
            {
                lock (SyncRoot)
                {

                    if (_instance == null)
                    {
                        _instance = new SharpStarMain();
                    }

                    return _instance;

                }
            }
        }

        private const string ConfigFile = "sharpstar.json";

        private const int DefaultListenPort = 21025;
        private const int DefaultServerPort = 21024;

        public SharpStarConfig Config { get; set; }

        public StarboundServer Server { get; private set; }

        public PluginManager PluginManager { get; set; }

        public SharpStarMain()
        {
        }

        public void Start()
        {

            string configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigFile);

            if (!File.Exists(configFile))
            {

                Config = new SharpStarConfig();
             
                Config.ConfigFile.ListenPort = DefaultListenPort;
                Config.ConfigFile.ServerPort = DefaultServerPort;

                Config.Save(configFile);

            }
            else
            {
                Config = SharpStarConfig.Load(configFile);
            }

            PluginManager = new PluginManager();

            Console.WriteLine("Listening on port {0}", Config.ConfigFile.ListenPort);

            Server = new StarboundServer(Config.ConfigFile.ListenPort, Config.ConfigFile.ServerPort);
            Server.Start();

            PluginManager.LoadPlugins();

        }

        public void Shutdown()
        {

            PluginManager.UnloadPlugins();
            
            Server.Stop();

        }

    }
}
