using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoinMonitor.Connections.KuCoin
{
    public class WebSocketSubscription
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("privateChannel")]
        public bool IsPrivateChannel { get; set; }

        [JsonProperty("response")]
        public bool IsResponse { get; set; }
    }
}