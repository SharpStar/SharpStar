using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Plugins
{
    public class ConsoleCommandAttribute : CommandAttribute
    {
        public ConsoleCommandAttribute(string cmdName) : base(cmdName)
        {
        }

        public ConsoleCommandAttribute(string cmdName, string cmdDesc) : base(cmdName, cmdDesc)
        {
        }
    }
}
