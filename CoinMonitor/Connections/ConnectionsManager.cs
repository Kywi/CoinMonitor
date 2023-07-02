using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Crypto;
using CoinMonitor.PriceCalculations;

namespace CoinMonitor.Connections
{
    public class ConnectionsManager : IDisposable
    {
        private readonly List<IConnectionManager> _connections;
        private readonly Manager _cryptoManager;
        private readonly List<Task> _tasks;
        private readonly PriceCalculator _priceCalculator;

        public ConnectionsManager(EventHandler<PricesCalculatedEventArgs> priceCalculated)
        {
            _tasks = new List<Task>();
            _connections = new List<IConnectionManager>
            {
                new Binance.Connection(),
                new WhiteBit.Connection(),
                new Bybit.Connection(),
                new Kraken.Connection(),
                new OKX.Connection(),
                new KuCoin.Connection()
            };

            _cryptoManager = new Manager(_connections.Select(socketManager => socketManager.GetExchange()).ToList());
            _priceCalculator = new PriceCalculator(_connections);
            _priceCalculator.PriceCalculated += priceCalculated;
        }

        public void Dispose()
        {
            _priceCalculator.Dispose();
            foreach (var connection in _connections)
                connection.Dispose();
        }

        public async void Connect()
        {
            await _cryptoManager.CalculateSupportedPairs();
            foreach (var socketManager in _connections)
                _tasks.Add(Task.Run(socketManager.StartAsync));

            _priceCalculator.Start();
        }
    }
}
