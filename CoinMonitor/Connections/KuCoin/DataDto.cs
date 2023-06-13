using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoinMonitor.Connections.KuCoin;

public class DataDto
{
    [JsonProperty("bids")]
    public List<List<decimal>> Bid { get; set; }

    [JsonProperty("asks")]
    public List<List<decimal>> Ask { get; set; }
}
