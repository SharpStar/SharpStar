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

            return config;
        }

        public void Save(string to)
        {
            File.WriteAllText(to, JsonConvert.SerializeObject(ConfigFile, Formatting.Indented));
        }
    }
}