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

            SharpStarLogger.DefaultLogger.Error(ex.Message);

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
