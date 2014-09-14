using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using SharpStar.Lib;
using SharpStar.Lib.Database;
using SharpStar.Lib.Extensions;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Mono;
using SharpStar.Lib.Plugins;

namespace SharpStar
{
    internal class Program
    {

        private static SharpStarLogger Logger;

        [HandleProcessCorruptedStateExceptions]
        private static void Main(string[] args)
        {

            XmlConfigurator.Configure();

            Logger = SharpStarLogger.DefaultLogger;

            SharpStarMain m = SharpStarMain.Instance;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Version ver = Assembly.GetExecutingAssembly().GetName().Version;

            Logger.Info("SharpStar Version {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);

            if (MonoHelper.IsRunningOnMono())
            {
                string monoVer = MonoHelper.GetMonoVersion();

                if (!string.IsNullOrEmpty(monoVer))
                {
                    Logger.Info("Running on Mono version {0}", monoVer);

                    if (!monoVer.StartsWith("3.2"))
                    {
                        Logger.Warn("You are running a version of Mono that has not been tested with SharpStar!");
                        Logger.Warn("SharpStar has been tested with Mono version 3.2.8. Versions other than that are not supported and may cause problems!");
                    }

                }
            }


            m.Start();

            while (true)
            {
                string line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                    continue;

                string[] cmd = line.Split(' ');

                if (cmd.Length == 0)
                    continue;

                SharpStarUser user;

                switch (cmd[0])
                {
                    case "loadplugin":

                        if (cmd.Length != 2)
                        {
                            Logger.Info("Syntax: loadplugin <file> (where file is in the \"{0}\" folder)",
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
                                Logger.Error(ex.Message);
                            }
                        }

                        break;

                    case "unloadplugin":

                        if (cmd.Length != 2)
                        {
                            Logger.Info("Syntax: unloadplugin <file>");
                        }
                        else
                        {
                            try
                            {
                                m.PluginManager.UnloadPlugin(Path.Combine(PluginManager.PluginDirectoryPath, cmd[1]));
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.Message);
                            }
                        }

                        break;

                    case "reloadplugins":

                        m.PluginManager.Reload();

                        break;

                    case "updateplugins":

                        Logger.Info("Updating Plugins...");

                        m.PluginManager.CSPluginManager.UpdatePlugins();

                        Logger.Info("Finished updating plugins!");

                        break;

                    case "updateplugin":

                        string updPlugin = string.Join(" ", cmd.Skip(1));

                        if (m.PluginManager.CSPluginManager.UpdatePlugin(updPlugin))
                            Logger.Info("Plugin {0} updated!", updPlugin);
                        else
                            Logger.Error("Error updating plugin {0}", updPlugin);

                        break;

                    case "installplugin":

                        string instPlugin = string.Join(" ", cmd.Skip(1));

                        m.PluginManager.CSPluginManager.InstallPlugin(instPlugin);

                        break;

                    case "uninstallplugin":

                        string uninstPlugin = string.Join(" ", cmd.Skip(1));

                        m.PluginManager.CSPluginManager.UninstallPlugin(uninstPlugin);

                        break;

                    case "unloadplugins":

                        m.PluginManager.UnloadPlugins();

                        break;

                    case "exit":

                        m.Shutdown();

                        Environment.Exit(0);

                        break;

                    case "addperm":

                        if (cmd.Length == 4)
                        {
                            user = m.Database.GetUser(cmd[1]);

                            if (user == null)
                            {
                                Logger.Info("User does not exist!");
                            }
                            else
                            {
                                bool allowed;

                                bool.TryParse(cmd[3], out allowed);

                                m.Database.AddPlayerPermission(user.Id, cmd[2], allowed);

                                Logger.Info("Added permission to user {0}!", user.Username);
                            }

                        }
                        else
                        {
                            Logger.Info("Syntax: addperm <username> <permission> <allowed>");
                        }

                        break;

                    case "removeperm":

                        if (cmd.Length == 3)
                        {
                            user = m.Database.GetUser(cmd[1]);

                            if (user == null)
                            {
                                Logger.Info("User does not exist!");
                            }
                            else
                            {
                                m.Database.DeletePlayerPermission(user.Id, cmd[2]);

                                Logger.Info("Permission removed from {0}!", user.Username);
                            }

                        }
                        else
                        {
                            Logger.Info("Syntax: removeperm <username> <permission>");
                        }

                        break;

                    case "op":

                        if (cmd.Length == 2)
                        {
                            user = m.Database.GetUser(cmd[1]);

                            if (user == null)
                            {
                                Logger.Info("User does not exist!");
                            }
                            else
                            {
                                m.Database.ChangeAdminStatus(user.Id, true);

                                Logger.Info("{0} is now an admin!", user.Username);
                            }
                        }

                        break;

                    case "deop":

                        user = m.Database.GetUser(cmd[1]);

                        if (user == null)
                        {
                            Logger.Info("User does not exist!");
                        }
                        else
                        {
                            m.Database.ChangeAdminStatus(user.Id, false);

                            Logger.Info("{0} is no longer an admin!", user.Username);
                        }

                        break;

                    default:

                        if (cmd.Length > 1)
                            m.PluginManager.PassConsoleCommand(cmd[0], new string(line.Skip(cmd[0].Length + 1).ToArray()).Split(' '));
                        else
                            m.PluginManager.PassConsoleCommand(cmd[0], new string[0]);

                        break;
                }
            }
        }

        [HandleProcessCorruptedStateExceptions]
        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            e.Exception.LogError();
        }

        [HandleProcessCorruptedStateExceptions]
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            (((Exception)e.ExceptionObject)).LogError();
        }
    }
}