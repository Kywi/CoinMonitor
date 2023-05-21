using Newtonsoft.Json;

namespace CoinMonitor.Connections.OKX
{
    public class DataDto
    {
        [JsonProperty("instId")]
        public string Symbol { get; set; }

        [JsonProperty("last")]
        public decimal Price { get; set; }
    }
    public class TickerDto
    {
        [JsonProperty("data")]
        public DataDto[] Data { get; set; }
    }

}