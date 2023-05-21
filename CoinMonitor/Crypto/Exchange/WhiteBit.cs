using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public class WhiteBit : IExchange
    {
        private readonly string _url;

        public List<TradingPair> SupportedCoins { get; private set; }

        public WhiteBit()
        {
            SupportedCoins = new List<TradingPair>();
            _url = "https://whitebit.com/api/v4/public/markets";
        }

        public void SetSupportedPairs(List<TradingPair> supportedCoins)
        {
            SupportedCoins = supportedCoins;
        }

        public async Task<HashSet<TradingPair>> RequestForSupportedPairs()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(_url);

            var content = await response.Content.ReadAsStringAsync();
            var markets = JArray.Parse(content);

            var coinNames = new HashSet<TradingPair>();
            foreach (var market in markets)
            {
                var names = market["name"].ToString().Split('_');

                var pair = new TradingPair(names[0], names[1]);
                if (TradingPair.IsSupportedPair(pair))
                    coinNames.Add(pair);
            }

            return coinNames;
        }
    }
}