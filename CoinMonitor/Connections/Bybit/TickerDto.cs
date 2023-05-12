using Newtonsoft.Json;

namespace CoinMonitor.Connections.Bybit
{
    public class DataDto
    {
        [JsonProperty("symbol")]
        public string TradingPair { get; set; }

        [JsonProperty("lastPrice")]
        public decimal ClosePrice { get; set; }
    }

    public class TickerDto
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("data")]
        public DataDto Data { get; set; }
    }
}