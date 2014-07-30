using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using SharpStar.Lib.Mono;

namespace SharpStar.Lib.Logging
{
    public class SharpStarLogger
    {

        private static SharpStarLogger _instance;

        private static readonly object _locker = new object();

        public static SharpStarLogger DefaultLogger
        {
            get
            {
                lock (_locker)
                {

                    if (_instance == null)
                        _instance = new SharpStarLogger();

                    return _instance;
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static SharpStarLogger()
        {

            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%level - %message%newline";
            layout.ActivateOptions();

            IAppender appender;

            if (MonoHelper.IsRunningOnMono())
            {

                AnsiColorTerminalAppender ansiColor = new AnsiColorTerminalAppender();
                ansiColor.AddMapping(new AnsiColorTerminalAppender.LevelColors { Level = Level.Info, ForeColor = AnsiColorTerminalAppender.AnsiColor.White, BackColor = AnsiColorTerminalAppender.AnsiColor.Green });
                ansiColor.AddMapping(new AnsiColorTerminalAppender.LevelColors { Level = Level.Debug, ForeColor = AnsiColorTerminalAppender.AnsiColor.White, BackColor = AnsiColorTerminalAppender.AnsiColor.Blue });
                ansiColor.AddMapping(new AnsiColorTerminalAppender.LevelColors { Level = Level.Warn, ForeColor = AnsiColorTerminalAppender.AnsiColor.Yellow, BackColor = AnsiColorTerminalAppender.AnsiColor.Magenta });
                ansiColor.AddMapping(new AnsiColorTerminalAppender.LevelColors { Level = Level.Error, ForeColor = AnsiColorTerminalAppender.AnsiColor.Yellow, BackColor = AnsiColorTerminalAppender.AnsiColor.Red });

                ansiColor.Layout = layout;
                ansiColor.ActivateOptions();

                appender = ansiColor;

            }
            else
            {

                ColoredConsoleAppender colorAppender = new ColoredConsoleAppender();
                colorAppender.AddMapping(new ColoredConsoleAppender.LevelColors { Level = Level.Info, ForeColor = ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity, BackColor = ColoredConsoleAppender.Colors.Green });
                colorAppender.AddMapping(new ColoredConsoleAppender.LevelColors { Level = Level.Debug, ForeColor = ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity, BackColor = ColoredConsoleAppender.Colors.Blue });
                colorAppender.AddMapping(new ColoredConsoleAppender.LevelColors { Level = Level.Warn, ForeColor = ColoredConsoleAppender.Colors.Yellow | ColoredConsoleAppender.Colors.HighIntensity, BackColor = ColoredConsoleAppender.Colors.Purple });
                colorAppender.AddMapping(new ColoredConsoleAppender.LevelColors { Level = Level.Error, ForeColor = ColoredConsoleAppender.Colors.Yellow | ColoredConsoleAppender.Colors.HighIntensity, BackColor = ColoredConsoleAppender.Colors.Red });

                colorAppender.Layout = layout;
                colorAppender.ActivateOptions();

                appender = colorAppender;

            }

            ((Logger)Log.Logger).AddAppender(appender);

        }


        public string PluginName { get; set; }

        private SharpStarLogger()
        {
        }

        public SharpStarLogger(string pluginName)
        {
            PluginName = pluginName;
        }

        public void Debug(string format, params object[] args)
        {
            if (SharpStarMain.Instance.Config.ConfigFile.ShowDebug)
                Log.DebugFormat(format, args);
        }

        public void Info(string format, params object[] args)
        {
            Log.InfoFormat(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Log.WarnFormat(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log.ErrorFormat(format, args);
        }

    }

    public static class StringExtensions
    {

        public static string PrependPluginName(this string str, string pluginName)
        {
            if (!string.IsNullOrEmpty(pluginName))
                return "[" + str + "] " + pluginName;

            return str;
        }

    }
}
