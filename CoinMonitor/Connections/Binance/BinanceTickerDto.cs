using Newtonsoft.Json;

namespace CoinMonitor.Connections.Binance
{
    public class BinanceTickerDto
    {
        [JsonProperty("s")]
        public string Symbol { get; set; }

        [JsonProperty("c")]
        public decimal Price { get; set; }

        [JsonProperty("C")]
        public double CloseTime { get; set; }
    }
}