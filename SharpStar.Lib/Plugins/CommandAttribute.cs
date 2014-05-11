using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Plugins
{
    public class CommandAttribute : SharpStarObjectAttribute
    {

        public string CommandName { get; set; }

        public string CommandDescription { get; set; }

        public CommandAttribute(string cmdName)
        {
            CommandName = cmdName;
        }

        public CommandAttribute(string cmdName, string cmdDesc)
            : this(cmdName)
        {
            CommandDescription = cmdDesc;
        }

    }
}
