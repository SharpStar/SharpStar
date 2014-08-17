using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharpStar.Lib.Logging;

namespace SharpStar.Lib.Extensions
{
    public static class ExceptionExtensions
    {

        public static void LogError(this Exception ex)
        {
            StackTrace st = new StackTrace(ex, true);
            StackFrame[] sf = st.GetFrames();

            SharpStarLogger.DefaultLogger.Error(ex.ToString());

            if (sf != null)
            {
                foreach (StackFrame f in sf)
                {
                    SharpStarLogger.DefaultLogger.Error(f.ToString());
                }
            }
        }

    }
}
