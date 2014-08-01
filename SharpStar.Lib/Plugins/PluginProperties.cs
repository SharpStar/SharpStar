// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharpStar.Lib.Plugins
{
    public class PluginProperties
    {
        public string PropertiesFile { get; set; }

        public JObject Properties { get; protected set; }

        public PluginProperties(string pluginName, string saveDir)
        {
            PropertiesFile = Path.Combine(saveDir, String.Format("{0}.json", pluginName));
        }

        public void Load()
        {
            if (!File.Exists(PropertiesFile))
                Properties = new JObject();
            else
                Properties = JObject.Parse(File.ReadAllText(PropertiesFile));
        }

        public string GetString(string key)
        {
            if (Properties[key] == null)
                return String.Empty;

            return Properties[key].ToObject<string>();
        }

        public void PutString(string key, string value)
        {
            Properties[key] = new JValue(value);
        }

        public bool GetBool(string key)
        {
            if (Properties[key] == null)
                return false;

            return Properties[key].ToObject<bool>();
        }

        public void PutBool(string key, bool boolean)
        {
            Properties[key] = new JValue(boolean);
        }

        public void PutArray(string key, object[] value)
        {
            Properties[key] = JArray.FromObject(value);
        }

        public object[] GetArray(string key)
        {
            if (Properties[key] == null)
                return null;

            return Properties[key].ToObject<object[]>();
        }

        public object Get(string key)
        {
            if (Properties[key] == null)
                return null;

            return Properties[key].ToObject<object>();
        }

        public JToken GetByIndex(JArray array, int index)
        {
            return array[index];
        }

        public JProperty GetProperty(string key)
        {
            if (Properties[key] == null)
                return null;

            return (JProperty) Properties[key];
        }

        public JArray GetPropertyArray(string key)
        {
            if (Properties[key] == null)
                return null;

            return (JArray) Properties[key];
        }

        public JObject GetPropertyObject(string key)
        {
            if (Properties[key] == null)
                return null;

            return (JObject) Properties[key];
        }

        public JToken GetPropertyValue(JProperty property)
        {
            return property.Value;
        }

        public void SetObject(JObject obj, object key, object value)
        {
            obj[key] = new JValue(value);
        }

        public void SetProperty(JProperty property, object key, JToken value)
        {
            property[key] = value;
        }

        public void Put(string key, object value)
        {
            Properties[key] = JToken.FromObject(value);
        }

        public void AppendArray(string key, object val)
        {
            if (Properties[key] == null)
                Properties[key] = new JArray();

            var arr = Properties[key].ToObject<JArray>();

            arr.Add(val);

            Properties[key] = arr;
        }

        public bool Remove(string key, object val)
        {
            if (Properties[key] == null)
                return false;

            if (Properties[key].Type == JTokenType.Array && Properties[key].HasValues)
            {
                var values = Properties[key].Values().ToList();

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
            if (Properties[key] == null)
                return false;

            Properties[key].Remove();

            return true;
        }

        public bool Contains(string key, object val)
        {
            if (Properties[key] == null)
                return false;

            if (Properties[key].Type == JTokenType.Array)
            {
                return Properties[key].Any(p => p.Value<string>() == val.ToString());
            }

            return false;
        }

        public void Save()
        {
            File.WriteAllText(PropertiesFile, Properties.ToString(Formatting.Indented));
        }
    }
}