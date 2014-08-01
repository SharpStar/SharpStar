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
using Newtonsoft.Json.Linq;

namespace SharpStar.Lib.Plugins
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