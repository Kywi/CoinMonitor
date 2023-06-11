using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.WhiteBit
{
    public class TickerDto
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public List<object> Params { get; set; }
    }
}