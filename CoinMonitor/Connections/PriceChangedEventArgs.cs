using System;

namespace CoinMonitor.Connections
{
    public class PriceChangedEventArgs : EventArgs
    {
        public string ExchangeName { get; }
        public string Symbol { get; }
        public decimal Price { get; }

        public PriceChangedEventArgs(string symbol, decimal price, string exchangeName)
        {
            Symbol = symbol;
            Price = price;
            ExchangeName = exchangeName;
        }
    }
}