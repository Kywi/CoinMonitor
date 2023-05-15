﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;

namespace CoinMonitor.Crypto
{
    public class Manager
    {
        private readonly List<IExchange> _exchanges;

        public Manager(List<IExchange> exchanges)
        {
            _exchanges = exchanges;
        }

        public async Task CalculateSupportedCoins()
        {
            var exchangeSupportedCoins = new Dictionary<IExchange, HashSet<string>>();
            foreach (var exchange in _exchanges)
                exchangeSupportedCoins.Add(exchange, await exchange.RequestForSupportedCoins());

            foreach (var exchangeCoins in exchangeSupportedCoins)
                exchangeCoins.Value.RemoveWhere(coin => exchangeSupportedCoins.All(ex =>
                {
                    if (exchangeCoins.Key == ex.Key)
                        return true;

                    return !ex.Value.Contains(coin);
                }));

            foreach (var exchangeCoins in exchangeSupportedCoins)
                exchangeCoins.Key.SetSupportedCoins(exchangeCoins.Value.ToList());
        }
    }
}