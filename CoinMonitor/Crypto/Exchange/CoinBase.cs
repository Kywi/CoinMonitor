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

        public List<string> SupportedCoins { get; private set; }

        public CoinBase()
        {
            SupportedCoins = new List<string>();
            _url = "https://api.exchange.coinbase.com/products";
        }

        public void SetSupportedCoins(List<string> supportedCoins)
        {
            SupportedCoins = supportedCoins;
        }

        public async Task<HashSet<string>> RequestForSupportedCoins()
        {
            var client = new HttpClient();
            var productValue = new ProductInfoHeaderValue("CoinMonitor", "1.0");
            client.DefaultRequestHeaders.UserAgent.Add(productValue);
            var response = await client.GetAsync(_url);

            var content = await response.Content.ReadAsStringAsync();
            var markets = JArray.Parse(content);

            var coinNames = new HashSet<string>();
            foreach (var market in markets)
            {
                var baseCoin = market["base_currency"].ToString();
                var secondCoin = market["quote_currency"].ToString();
                if (secondCoin == "USDT")
                    coinNames.Add(baseCoin);
            }

            coinNames.Remove("USDT");
            return coinNames;
        }
    }
}