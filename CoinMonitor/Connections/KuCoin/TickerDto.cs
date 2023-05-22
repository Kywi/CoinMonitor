using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.KuCoin
{
    public class TickerDto
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
