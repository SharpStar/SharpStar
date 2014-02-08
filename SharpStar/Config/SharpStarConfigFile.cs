using Newtonsoft.Json;

namespace SharpStar.Config
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SharpStarConfigFile
    {

        [JsonProperty("listenPort")]
        public int ListenPort { get; set; }

        [JsonProperty("serverPort")]
        public int ServerPort { get; set; }

    }
}
