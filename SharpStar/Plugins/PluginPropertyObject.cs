using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SharpStar.Plugins
{
    public class PluginPropertyObject : JObject
    {

        public PluginPropertyObject()
        {
        }

        public PluginPropertyObject(JObject obj)
            : base(obj)
        {
        }

        public PluginPropertyObject(object content)
            : base(content)
        {
        }

        public JToken GetByIndex(int index)
        {
            return this[index];
        }

        public JToken GetByKey(object key)
        {
            return this[key];
        }

        public void Set(object key, JToken value)
        {
            this[key] = value;
        }

    }
}
