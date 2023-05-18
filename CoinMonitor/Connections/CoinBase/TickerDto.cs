using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.CoinBase
{
    public class TickerDto
    {
        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}