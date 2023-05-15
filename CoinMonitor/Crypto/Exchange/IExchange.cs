using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public interface IExchange
    {
        public void SetSupportedCoins(List<string> supportedCoins);

        public Task<HashSet<string>> RequestForSupportedCoins();
    }
}