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
using Newtonsoft.Json.Linq;

namespace SharpStar.Lib.Plugins
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