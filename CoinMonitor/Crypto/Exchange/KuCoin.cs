using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public class KuCoin : IExchange
    {
        private readonly string _url;

        public List<TradingPair> SupportedPairs { get; private set; }

        public KuCoin()
        {
            SupportedPairs = new List<TradingPair>();
            _url = "https://api.kucoin.com/api/v2/symbols";
        }

        public static string GetName()
        {
            return "KuCoin";
        }

        public void SetSupportedPairs(List<TradingPair> supportedPairs)
        {
            SupportedPairs = supportedPairs;
        }

        public async Task<HashSet<TradingPair>> RequestForSupportedPairs()
        {
            var client = new HttpClient();

            var response = await client.GetAsync(_url);

            var content = await response.Content.ReadAsStringAsync();

            var markets = (JArray)JObject.Parse(content)["data"];
            var coinNames = new HashSet<TradingPair>();
            foreach (var market in markets)
            {
                var baseCoin = market["baseCurrency"].ToString();
                var quote = market["quoteCurrency"].ToString();

                var pair = new TradingPair(baseCoin, quote);
                if (TradingPair.IsSupportedPair(pair))
                    coinNames.Add(pair);
            }

            return coinNames;
        }
    }
}