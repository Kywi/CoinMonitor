using Newtonsoft.Json;

namespace CoinMonitor.Connections.KuCoin;

public class Data
{
    [JsonProperty("price")]
    public decimal Price { get; set; }
}
