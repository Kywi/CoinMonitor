using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoinMonitor.Connections.WhiteBit
{
    public class WebSocketSubscription
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public List<object> Params { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}