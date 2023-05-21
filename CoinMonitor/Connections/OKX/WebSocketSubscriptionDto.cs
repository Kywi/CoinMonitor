using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoinMonitor.Connections.OKX
{
    public class WebSocketSubscriptionDto
    {
        [JsonProperty("op")]
        public string Operation { get; set; } = string.Empty;

        [JsonProperty("args")]
        public ArgsDto[] Parameters { get; set; }
    }

    public class ArgsDto
    {
        [JsonProperty("channel")]
        public string Channel { get; set; } = string.Empty;

        [JsonProperty("instId")]
        public string InstId { get; set; } = string.Empty;
    }
}