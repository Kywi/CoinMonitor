using Newtonsoft.Json;
using System;

namespace CoinMonitor.Connections.Bybit
{
    public class WebSocketSubscriptionDto
    {
        [JsonProperty("req_id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("op")]
        public string Operation { get; set; } = string.Empty;

        [JsonProperty("args")]
        public string[] Parameters { get; set; } = Array.Empty<string>();
    }
}