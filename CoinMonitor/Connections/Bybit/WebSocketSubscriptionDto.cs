using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoinMonitor.Connections.Bybit
{
    public class WebSocketSubscriptionDto
    {
        [JsonProperty("req_id")]
        public int Id { get; set; }

        [JsonProperty("op")]
        public string Operation { get; set; }

        [JsonProperty("args")]
        public List<string> Params { get; set; }
    }
}