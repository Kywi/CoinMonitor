using System;

namespace CoinMonitor.Connections
{
    public class PriceChangedEventArgs : EventArgs
    {
        public string ExcgangeName { get; }
        public string Symbol { get; }
        public decimal Price { get; }

        public PriceChangedEventArgs(string symbol, decimal price, string excgangeName)
        {
            Symbol = symbol;
            Price = price;
            ExcgangeName = excgangeName;
        }
    }
}