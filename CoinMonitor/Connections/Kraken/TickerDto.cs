using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.Kraken
{
    public class TickerDto
    {
        [JsonProperty("c")]
        public List<decimal> Price { get; set; }
    }
}