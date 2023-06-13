using Newtonsoft.Json;

namespace CoinMonitor.Connections.Bybit
{
    public class TickerDto
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("data")]
        public DataDto Data { get; set; }
    }
}