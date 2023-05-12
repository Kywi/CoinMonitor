using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoinMonitor.Connections.Binance;
using CoinMonitor.Connections.Bybit;
using CoinMonitor.Connections.WhiteBit;

namespace CoinMonitor.Connections
{
    public class ConnectionsManager
    {
        private List<IWebSocketManager> _sockets = new List<IWebSocketManager>();

        private List<Task> _tasks = new List<Task>();

        public ConnectionsManager(EventHandler<PriceChangedEventArgs> priceUpdate)
        {
            _sockets.Add(new BinanceWebSocketManager());
            _sockets.Add(new WhiteBitWebSocketManager());
            _sockets.Add(new BybitWebSocketManager());

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