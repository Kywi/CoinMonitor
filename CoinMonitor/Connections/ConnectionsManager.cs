using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoinMonitor.Connections
{
    public class ConnectionsManager
    {
        private List<IWebSocketManager> _sockets = new List<IWebSocketManager>();

        private List<Task> _tasks = new List<Task>();

        public ConnectionsManager(List<string> symbols, EventHandler<PriceChangedEventArgs> priceUpdate)
        {
            _sockets.Add(new BinanceWebSocketManager(symbols));

            foreach (var socketManager in _sockets)
                socketManager.PriceUpdate += priceUpdate;
        }

        public void Connect()
        {
            foreach (var socketManager in _sockets)
            {
                _tasks.Add(socketManager.StartAsync());
            }
        }
    }
}