using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoinMonitor.Connections.Bybit;

public class DataDto
{
    [JsonProperty("s")]
    public string TradingPair { get; set; }

    [JsonProperty("b")]
    public List<List<decimal>> Bid { get; set; }

    [JsonProperty("a")]
    public List<List<decimal>> Ask { get; set; }
}
