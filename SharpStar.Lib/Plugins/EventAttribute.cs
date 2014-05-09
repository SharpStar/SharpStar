using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Plugins
{
    public class EventAttribute : SharpStarObjectAttribute
    {

        public string[] EventNames { get; set; }

        public EventAttribute(params string[] evtNames)
        {
            EventNames = evtNames;
        }

    }
}
