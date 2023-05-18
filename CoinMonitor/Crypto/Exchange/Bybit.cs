using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public class Bybit : IExchange
    {
        private readonly string _url;

        public List<string> SupportedCoins { get; private set; }

        public Bybit()
        {
            _url = "https://api.bybit.com/v5/market/tickers?category=spot";
        }

        public void SetSupportedCoins(List<string> supportedCoins)
        {
            SupportedCoins = supportedCoins;
        }

        public async Task<HashSet<string>> RequestForSupportedCoins()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(_url);

            var content = await response.Content.ReadAsStringAsync();
            var symbols = JObject.Parse(content);
            var result = (JArray)symbols["result"]["list"];

            var coinNames = new HashSet<string>();
            foreach (var symbol in result)
            {
                var coinName = symbol["symbol"].ToString();
                if (coinName.Contains("USDT"))
                    coinNames.Add(coinName.Substring(0, coinName.Length - 4));
            }

            coinNames.Remove("USD");
            coinNames.Remove("USDT");

            return coinNames;
        }
    }
}