using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public class Bybit : IExchange
    {
        private readonly string _url;

        public List<TradingPair> SupportedPairs { get; private set; }

        public Bybit()
        {
            SupportedPairs = new List<TradingPair>();
            _url = "https://api.bybit.com/v5/market/tickers?category=spot";
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

            var symbols = JObject.Parse(content);
            var result = (JArray)symbols["result"]["list"];
            var coinNames = new HashSet<TradingPair>();
            foreach (var symbol in result)
            {
                var pairStr = symbol["symbol"].ToString();

                if (!pairStr.Contains("USDT") || !(pairStr.Substring(pairStr.Length - 4, 4) == "USDT"))
                    continue;

                var baseCoin = pairStr.Substring(0, pairStr.Length - 4);
                var pair = new TradingPair(baseCoin, "USDT");
                if (TradingPair.IsSupportedPair(pair))
                    coinNames.Add(pair);
            }

            return coinNames;
        }
    }
}