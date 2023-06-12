using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.Kraken
{
    public class TickerDto
    {
        [JsonProperty("as")]
        public List<List<decimal>> Ask { get; set; }

        [JsonProperty("bs")]
        public List<List<decimal>> Bid { get; set; }
    }
}