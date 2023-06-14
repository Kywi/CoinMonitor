using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoinMonitor.Connections.Binance;
using CoinMonitor.Crypto;
using CoinMonitor.Crypto.Exchange;

namespace CoinMonitor.Connections
{
    public class ConnectionsManager : IDisposable
    {
        private readonly List<IConnectionManager> _connections = new();
        private readonly Crypto.Manager _cryptoManager;

        private readonly List<Task> _tasks;

        public ConnectionsManager(EventHandler<PriceChangedEventArgs> priceUpdate)
        {
            _connections.Add(new Binance.Connection());
            _connections.Add(new WhiteBit.Connection());
            _connections.Add(new Bybit.Connection());
            _connections.Add(new Kraken.Connection());
            _connections.Add(new OKX.Connection());
            _connections.Add(new KuCoin.Connection());

            var exchangeList = new List<IExchange>();
            foreach (var socketManager in _connections)
            {
                socketManager.PriceUpdate += priceUpdate;
                exchangeList.Add(socketManager.GetExchange());
            }

            _cryptoManager = new Manager(exchangeList);
            _tasks = new List<Task>();
        }

        public void Dispose()
        {
            foreach (var connection in _connections)
                connection.Dispose();
        }

        public async void Connect()
        {
            await _cryptoManager.CalculateSupportedPairs();
            foreach (var socketManager in _connections)
                _tasks.Add(socketManager.StartAsync());
        }
    }
}