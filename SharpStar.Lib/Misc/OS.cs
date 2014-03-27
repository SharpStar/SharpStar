using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Misc
{
    public class OS
    {

        public static bool IsWindows
        {
            get
            {
                OperatingSystem os = Environment.OSVersion;

                PlatformID pid = os.Platform;

                switch (pid)
                {

                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        return true;

                    default:
                        return false;

                }
            }
        }

    }
}
