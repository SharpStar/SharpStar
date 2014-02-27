using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SharpStar.Plugins
{
    public class PluginProperty : JProperty
    {
        public PluginProperty(JProperty other) : base(other)
        {
        }

        public PluginProperty(string name, object content) : base(name, content)
        {
        }

        public JToken GetValue()
        {
            return Value;
        }

    }
}
