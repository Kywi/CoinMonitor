using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;

namespace CoinMonitor.Crypto
{
    public class Manager
    {
        private readonly List<IExchange> _exchanges;

        public static string[] SupportedExchanges = { Binance.GetName(), WhiteBit.GetName(), Bybit.GetName(), Kraken.GetName(), OKX.GetName(), KuCoin.GetName() };

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

            var binanceExchange = exchangeSupportedCoins.Select(pair => pair).First(exchangePairs => exchangePairs.Key is Binance);

            foreach (var exchangeCoins in exchangeSupportedCoins)
                if (exchangeCoins.Key is not Binance)
                    exchangeCoins.Value.RemoveWhere(coin => !binanceExchange.Value.Contains(coin));

            foreach (var exchangeCoins in exchangeSupportedCoins)
                exchangeCoins.Key.SetSupportedPairs(exchangeCoins.Value.ToList());
        }
    }
}