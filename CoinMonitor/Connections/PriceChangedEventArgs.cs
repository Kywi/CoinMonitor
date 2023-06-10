using System;

namespace CoinMonitor.Connections
{
    public class PriceChangedEventArgs : EventArgs
    {
        public string ExchangeName { get; }
        public string Symbol { get; }
        public decimal? Bid { get; }
        public decimal? Ask { get; }

        public PriceChangedEventArgs(string symbol, decimal? bid, decimal? ask, string exchangeName)
        {
            Symbol = symbol;
            Bid = bid;
            Ask = ask;
            ExchangeName = exchangeName;
        }
    }
}