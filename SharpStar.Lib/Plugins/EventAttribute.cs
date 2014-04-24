using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Plugins
{
    public class EventAttribute : SharpStarObjectAttribute
    {

        public string EventName { get; set; }

        public EventAttribute(string evtName)
        {
            EventName = evtName;
        }

    }
}
