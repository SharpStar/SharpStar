using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpStar.Lib;
using SharpStar.Lib.Database;
using SharpStar.Lib.Plugins;

namespace SharpStar
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SharpStarMain m = SharpStarMain.Instance;

            Version ver = Assembly.GetExecutingAssembly().GetName().Version;

            Console.WriteLine("SharpStar Version {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);

            m.Start();

            SharpStarUser user = null;

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

                    case "addperm":

                        if (cmd.Length == 4)
                        {

                            user = m.Database.GetUser(cmd[1]);

                            if (user == null)
                            {
                                Console.WriteLine("User does not exist!");
                            }
                            else
                            {

                                bool allowed;

                                bool.TryParse(cmd[3], out allowed);

                                m.Database.AddPlayerPermission(user.Id, cmd[2], allowed);

                                Console.WriteLine("Added permission to user {0}!", user.Username);
                            }

                        }
                        else
                        {
                            Console.WriteLine("Syntax: addperm <username> <permission> <allowed>");
                        }

                        break;

                    case "removeperm":

                        if (cmd.Length == 3)
                        {

                            user = m.Database.GetUser(cmd[1]);

                            if (user == null)
                            {
                                Console.WriteLine("User does not exist!");
                            }
                            else
                            {

                                m.Database.DeletePlayerPermission(user.Id, cmd[2]);

                                Console.WriteLine("Permission removed from {0}!", user.Username);

                            }

                        }
                        else
                        {
                            Console.WriteLine("Syntax: removeperm <username> <permission>");
                        }

                        break;

                    case "op":

                        if (cmd.Length == 2)
                        {

                            user = m.Database.GetUser(cmd[1]);

                            if (user == null)
                            {
                                Console.WriteLine("User does not exist!");
                            }
                            else
                            {

                                m.Database.ChangeAdminStatus(user.Id, true);

                                Console.WriteLine("{0} is now an admin!", user.Username);

                            }

                        }

                        break;

                    case "deop":

                        user = m.Database.GetUser(cmd[1]);

                        if (user == null)
                        {
                            Console.WriteLine("User does not exist!");
                        }
                        else
                        {

                            m.Database.ChangeAdminStatus(user.Id, false);

                            Console.WriteLine("{0} is no longer an admin!", user.Username);

                        }

                        break;

                    default:

                        m.PluginManager.PassChatCommand(null, cmd[0], new string(line.Skip(cmd[0].Length + 1).ToArray()).Split(' '));

                        break;
                }
            }
        }
    }
}