using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SharpStar.Plugins
{
    public class PluginPropertyArray : JArray
    {

        public PluginPropertyArray()
        {
        }

        public PluginPropertyArray(JArray arr)
            : base(arr)
        {
        }

        public PluginPropertyArray(object content)
            : base(content)
        {
        }

        public PluginPropertyArray(params object[] content)
            : base(content)
        {
        }

        public JToken GetByIndex(int index)
        {
            return this[index];
        }

    }
}
