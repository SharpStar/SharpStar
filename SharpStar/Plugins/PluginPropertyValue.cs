using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SharpStar.Plugins
{
    public class PluginPropertyValue : JValue
    {
        public PluginPropertyValue(JValue other) : base(other)
        {
        }

        public PluginPropertyValue(long value) : base(value)
        {
        }

        public PluginPropertyValue(decimal value) : base(value)
        {
        }

        public PluginPropertyValue(char value) : base(value)
        {
        }

        public PluginPropertyValue(ulong value) : base(value)
        {
        }

        public PluginPropertyValue(double value) : base(value)
        {
        }

        public PluginPropertyValue(float value) : base(value)
        {
        }

        public PluginPropertyValue(DateTime value) : base(value)
        {
        }

        public PluginPropertyValue(DateTimeOffset value) : base(value)
        {
        }

        public PluginPropertyValue(bool value) : base(value)
        {
        }

        public PluginPropertyValue(string value) : base(value)
        {
        }

        public PluginPropertyValue(Guid value) : base(value)
        {
        }

        public PluginPropertyValue(TimeSpan value) : base(value)
        {
        }

        public PluginPropertyValue(object value) : base(value)
        {
        }

    }
}
