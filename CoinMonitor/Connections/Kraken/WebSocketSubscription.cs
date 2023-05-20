using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace CoinMonitor.Connections.Kraken
{
    public class WebSocketSubscription
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("pair")]
        public List<string> Pairs { get; set; }

        [JsonProperty("subscription")]
        public Subscription Subscription { get; set; }
    }
}