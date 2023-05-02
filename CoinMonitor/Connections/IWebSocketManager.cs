using System;
using System.Threading.Tasks;

namespace CoinMonitor.Connections
{
    public interface IWebSocketManager
    {
        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public Task StartAsync();
    }
}