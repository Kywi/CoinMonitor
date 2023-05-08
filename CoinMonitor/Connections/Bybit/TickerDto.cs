using Newtonsoft.Json;

namespace CoinMonitor.Connections.Bybit
{
    public class DataDto
    {
        [JsonProperty("s")]
        public string TradingPair { get; set; }

        [JsonProperty("o")]
        public decimal OpenPrice { get; set; }
    }

    public class TickerDto
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("data")]
        public DataDto Data { get; set; }
    }
}