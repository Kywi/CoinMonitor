using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoinMonitor.Connections.CoinBase
{
    public class WebSocketSubscription
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("product_ids")]
        public List<string> ProductIds { get; set; }

        [JsonProperty("channels")]
        public List<string> Channels { get; set; }

    }
}