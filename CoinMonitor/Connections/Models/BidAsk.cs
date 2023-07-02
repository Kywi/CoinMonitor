namespace CoinMonitor.Connections.Models;

public struct BidAsk
{
    public decimal Bid;
    public decimal Ask;

    public BidAsk(decimal bid, decimal ask)
    {
        Bid = bid;
        Ask = ask;
    }
}
