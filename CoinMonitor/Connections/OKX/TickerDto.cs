using Newtonsoft.Json;

namespace CoinMonitor.Connections.OKX
{
    public class DataDto
    {
        [JsonProperty("instId")]
        public string Symbol { get; set; }

        [JsonProperty("bidPx")]
        public decimal Bid { get; set; }

        [JsonProperty("bidSz")]
        public decimal BidSz { get; set; }

        [JsonProperty("askPx")]
        public decimal Ask { get; set; }

        [JsonProperty("askSz")]
        public decimal AskSz { get; set; }
    }
    public class TickerDto
    {
        [JsonProperty("data")]
        public DataDto[] Data { get; set; }
    }

}