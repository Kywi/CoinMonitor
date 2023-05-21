using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public class CoinBase : IExchange
    {
        private readonly string _url;

        public List<TradingPair> SupportedCoins { get; private set; }

        public CoinBase()
        {
            SupportedCoins = new List<TradingPair>();
            _url = "https://api.exchange.coinbase.com/products";
        }

        public void SetSupportedPairs(List<TradingPair> supportedCoins)
        {
            SupportedCoins = supportedCoins;
        }

        public async Task<HashSet<TradingPair>> RequestForSupportedPairs()
        {
            var client = new HttpClient();
            var productValue = new ProductInfoHeaderValue("CoinMonitor", "1.0");
            client.DefaultRequestHeaders.UserAgent.Add(productValue);
            var response = await client.GetAsync(_url);

            var content = await response.Content.ReadAsStringAsync();
            var markets = JArray.Parse(content);

            var coinNames = new HashSet<TradingPair>();
            foreach (var market in markets)
            {
                var baseCoin = market["base_currency"].ToString();
                var quote = market["quote_currency"].ToString();

                var pair = new TradingPair(baseCoin, quote);
                if (TradingPair.IsSupportedPair(pair))
                    coinNames.Add(pair);
            }

            return coinNames;
        }
    }
}