using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace SharpStar.Lib.Misc
{
    public static class Python
    {

        public static string GetPythonInstallDir()
        {

            if (!OS.IsWindows)
                return null;

            RegistryKey pythonKey;

            if (Environment.Is64BitOperatingSystem)
            {
                pythonKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Python\PythonCore"); ;
            }
            else
            {
                pythonKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Python\PythonCore");
            }
            
            if (pythonKey == null)
                return null;

            string[] keys = pythonKey.GetSubKeyNames();

            if (keys.Length == 0)
                return null;

            RegistryKey pyInstallKey = pythonKey.OpenSubKey(String.Format(@"{0}\InstallPath", keys[0]));


            if (pyInstallKey == null)
            {

                pythonKey.Close();
                pythonKey.Dispose();

                return null;

            }

            string installDir = (string)pyInstallKey.GetValue(null);

            pyInstallKey.Close();
            pythonKey.Close();
            
            pythonKey.Dispose();
            pyInstallKey.Dispose();

            return installDir;

        }

    }
}
