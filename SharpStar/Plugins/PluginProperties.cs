using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharpStar.Plugins
{
    public class PluginProperties
    {

        public string PropertiesFile { get; set; }

        private JObject _properties;

        public PluginProperties(string pluginName, string saveDir)
        {
            PropertiesFile = Path.Combine(saveDir, String.Format("{0}.json", pluginName));
        }

        public void Load()
        {

            if (!File.Exists(PropertiesFile))
                _properties = new JObject();
            else
                _properties = JObject.Parse(File.ReadAllText(PropertiesFile));

        }

        public void Put(string key, string value)
        {
            _properties[key] = new JValue(value);
        }

        public string Get(string key)
        {

            if (_properties[key] == null)
                return String.Empty;

            return _properties[key].ToObject<string>();
        
        }

        public void Save()
        {
            File.WriteAllText(PropertiesFile, _properties.ToString(Formatting.Indented));
        }

    }
}
