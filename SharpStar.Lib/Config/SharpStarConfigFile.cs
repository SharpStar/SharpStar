using Newtonsoft.Json;

namespace SharpStar.Lib.Config
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SharpStarConfigFile
    {
        [JsonProperty("listenPort")]
        public int ListenPort { get; set; }

        [JsonProperty("serverPort")]
        public int ServerPort { get; set; }

        [JsonProperty("PythonLibLocation")]
        public string PythonLibLocation { get; set; }

    }
}