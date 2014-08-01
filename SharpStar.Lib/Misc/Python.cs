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
            {

                pythonKey.Close();
                pythonKey.Dispose();

                return null;

            }

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
