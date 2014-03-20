using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpStar.Lib;
using SharpStar.Lib.Plugins;

namespace SharpStar
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SharpStarMain m = SharpStarMain.Instance;

            Version ver = Assembly.GetExecutingAssembly().GetName().Version;

            Console.WriteLine("SharpStar Version {0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);

            m.Start();

            bool running = true;
            while (running)
            {
                string line = Console.ReadLine();

                string[] cmd = line.Split(' ');

                if (cmd.Length == 0)
                    continue;

                switch (cmd[0])
                {
                    case "loadplugin":

                        if (cmd.Length != 2)
                        {
                            Console.WriteLine("Syntax: loadplugin <file> (where file is in the \"{0}\" folder)",
                                PluginManager.PluginDirectory);
                        }
                        else
                        {
                            try
                            {
                                m.PluginManager.LoadPlugin(Path.Combine(PluginManager.PluginDirectoryPath, cmd[1]));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                        break;

                    case "unloadplugin":

                        if (cmd.Length != 2)
                        {
                            Console.WriteLine("Syntax: unloadplugin <file>");
                        }
                        else
                        {
                            try
                            {
                                m.PluginManager.UnloadPlugin(Path.Combine(PluginManager.PluginDirectoryPath, cmd[1]));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                        break;

                    case "reloadplugins":

                        m.PluginManager.Reload();

                        break;

                    case "unloadplugins":

                        m.PluginManager.UnloadPlugins();

                        break;

                    case "exit":

                        m.Shutdown();

                        running = false;

                        break;

                    default:

                        m.PluginManager.PassChatCommand(null, cmd[0],
                            new string(line.Skip(cmd[0].Length + 1).ToArray()).Split(' '));

                        break;
                }
            }
        }
    }
}