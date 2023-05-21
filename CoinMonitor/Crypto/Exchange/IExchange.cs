using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public interface IExchange
    {
        public void SetSupportedPairs(List<TradingPair> supportedCoins);

        public Task<HashSet<TradingPair>> RequestForSupportedPairs();
    }
}