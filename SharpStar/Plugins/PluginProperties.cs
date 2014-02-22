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

        public string GetString(string key)
        {

            if (_properties[key] == null)
                return String.Empty;

            return _properties[key].ToObject<string>();

        }

        public void PutString(string key, string value)
        {
            _properties[key] = new JValue(value);
        }

        public bool GetBool(string key)
        {

            if (_properties[key] == null)
                return false;

            return _properties[key].ToObject<bool>();

        }

        public void PutBool(string key, bool boolean)
        {
            _properties[key] = new JValue(boolean);
        }

        public void PutArray(string key, object[] value)
        {
            _properties[key] = JArray.FromObject(value);
        }

        public string[] GetArray(string key)
        {

            if (_properties[key] == null)
                return null;

            return _properties[key].ToObject<string[]>();

        }

        public void AppendArray(string key, object val)
        {

            if (_properties[key] == null)
                _properties[key] = new JArray();

            JArray arr = _properties[key].ToObject<JArray>();

            arr.Add(val);

            _properties[key] = arr;

        }

        public bool Remove(string key, object val)
        {

            if (_properties[key] == null)
                return false;

            if (_properties[key].Type == JTokenType.Array && _properties[key].HasValues)
            {

                var values = _properties[key].Values().ToList();

                for (int i = 0; i < values.Count; i++)
                {

                    JToken token = values[i];

                    if (token.Value<string>() == val.ToString())
                        token.Remove();

                }

                return true;

            }

            return false;

        }

        public bool Remove(string key)
        {

            if (_properties[key] == null)
                return false;

            _properties[key].Remove();

            return true;

        }

        public bool Contains(string key, object val)
        {

            if (_properties[key] == null)
                return false;

            if (_properties[key].Type == JTokenType.Array)
            {
                return _properties[key].Any(p => p.Value<string>() == val.ToString());
            }

            return false;

        }

        public void Save()
        {
            File.WriteAllText(PropertiesFile, _properties.ToString(Formatting.Indented));
        }

    }
}
