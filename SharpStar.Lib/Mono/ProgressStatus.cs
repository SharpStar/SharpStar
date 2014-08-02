using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Addins;
using SharpStar.Lib.Logging;

namespace SharpStar.Lib.Mono
{
    public class ProgressStatus : IProgressStatus
    {

        private readonly SharpStarLogger Logger = SharpStarLogger.DefaultLogger;

        private double lastProgress;

        public void SetMessage(string msg)
        {
        }

        public void SetProgress(double progress)
        {
        }

        public void Log(string msg)
        {
        }

        public void ReportWarning(string message)
        {
            Logger.Warn(message);
        }

        public void ReportError(string message, Exception exception)
        {
            if (!string.IsNullOrEmpty(message))
                Logger.Error(message);
        }

        public void Cancel()
        {
            Logger.Warn("Cancelled!");
        }

        public int LogLevel { get; private set; }
        public bool IsCanceled { get; private set; }
    }
}
