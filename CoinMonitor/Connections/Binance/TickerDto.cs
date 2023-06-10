using Newtonsoft.Json;

namespace CoinMonitor.Connections.Binance
{
    public class TickerDto
    {
        [JsonProperty("s")]
        public string Symbol { get; set; }

        [JsonProperty("b")]
        public decimal Bid { get; set; }

        [JsonProperty("a")]
        public decimal Ask { get; set; }

        [JsonProperty("B")]
        public decimal BidQty { get; set; }

        [JsonProperty("A")]
        public decimal AskQty { get; set; }

    }
}