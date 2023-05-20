using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoinMonitor.Crypto;
using CoinMonitor.Crypto.Exchange;

namespace CoinMonitor.Connections
{
    public class ConnectionsManager
    {
        private readonly List<IConnectionManager> _connections = new List<IConnectionManager>();
        private readonly Crypto.Manager _cryptoManager;

        private readonly List<Task> _tasks = new List<Task>();

        public ConnectionsManager(EventHandler<PriceChangedEventArgs> priceUpdate)
        {
            _connections.Add(new Binance.Connection());
            _connections.Add(new WhiteBit.Connection());
            _connections.Add(new Bybit.Connection());
            _connections.Add(new CoinBase.Connection());
            _connections.Add(new Kraken.Connection());

            var exchangeList = new List<IExchange>();
            foreach (var socketManager in _connections)
            {
                socketManager.PriceUpdate += priceUpdate;
                exchangeList.Add(socketManager.GetExchange());
            }

            _cryptoManager = new Manager(exchangeList);
        }

        public async void Connect()
        {
            await _cryptoManager.CalculateSupportedCoins();
            foreach (var socketManager in _connections)
                _tasks.Add(socketManager.StartAsync());
        }
    }
}