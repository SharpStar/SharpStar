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
using System.IO;
using Newtonsoft.Json;

namespace SharpStar.Lib.Config
{
    public class SharpStarConfig
    {
        public SharpStarConfigFile ConfigFile { get; set; }

        public SharpStarConfig()
        {
            ConfigFile = new SharpStarConfigFile();
        }

        public static SharpStarConfig Load(string configFile)
        {
            SharpStarConfig config = new SharpStarConfig();
            config.ConfigFile = JsonConvert.DeserializeObject<SharpStarConfigFile>(File.ReadAllText(configFile));
            config.Save(configFile);

            return config;
        }

        public void Save(string to)
        {
            File.WriteAllText(to, JsonConvert.SerializeObject(ConfigFile, Formatting.Indented));
        }
    }
}