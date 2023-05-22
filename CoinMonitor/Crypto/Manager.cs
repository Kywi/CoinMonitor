using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;

namespace CoinMonitor.Crypto
{
    public class Manager
    {
        private readonly List<IExchange> _exchanges;

        public static string[] SupportedExchanges = { "Binance", "WhiteBit", "Bybit", "CoinBase", "Kraken", "OKX", "KuCoin" };

        public Manager(List<IExchange> exchanges)
        {
            _exchanges = exchanges;
        }

        public async Task CalculateSupportedPairs()
        {
            var exchangeSupportedCoins = new Dictionary<IExchange, HashSet<TradingPair>>();
            foreach (var exchange in _exchanges)
                exchangeSupportedCoins.Add(exchange, await exchange.RequestForSupportedPairs());

            foreach (var exchangeCoins in exchangeSupportedCoins)
                exchangeCoins.Value.RemoveWhere(coin => exchangeSupportedCoins.All(ex =>
                {
                    if (exchangeCoins.Key == ex.Key)
                        return true;

                    return !ex.Value.Contains(coin);
                }));

            foreach (var exchangeCoins in exchangeSupportedCoins)
                exchangeCoins.Key.SetSupportedPairs(exchangeCoins.Value.ToList());
        }
    }
}