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
