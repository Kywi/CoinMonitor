using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoinMonitor.Connections.WhiteBit;

public class BidAskDto
{
    [JsonProperty("asks")]
    public List<List<string>> Ask { get; set; }

    [JsonProperty("bids")]
    public List<List<string>> Bid { get; set; }
}