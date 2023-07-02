using System.Collections.Generic;
using CoinMonitor.Connections.Models;

namespace CoinMonitor.PriceCalculations.Models;

public struct Coin
{
    public string Name;

    public Dictionary<string, BidAsk> ExchangeNameBidAsk;

    public Coin(string name)
    {
        Name = name;
        ExchangeNameBidAsk = new Dictionary<string, BidAsk>();
    }
}