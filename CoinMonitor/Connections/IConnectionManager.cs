using System;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;

namespace CoinMonitor.Connections
{
    public interface IConnectionManager
    {
        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public Task StartAsync();

        public IExchange GetExchange();
    }
}