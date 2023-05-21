using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    internal class OKX : IExchange
    {
        private readonly string _url;

        public List<TradingPair> SupportedPairs { get; private set; }

        public OKX()
        {
            SupportedPairs = new List<TradingPair>();
            _url = "https://www.okx.com/api/v5/market/tickers?instType=SPOT";
        }

        public void SetSupportedPairs(List<TradingPair> supportedCoins)
        {
            SupportedPairs = supportedCoins;
        }

        public async Task<HashSet<TradingPair>> RequestForSupportedPairs()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(_url);

            var content = await response.Content.ReadAsStringAsync();
            var symbols = JObject.Parse(content);
            var result = (JArray)symbols["data"];

            var coinNames = new HashSet<TradingPair>();
            foreach (var symbol in result)
            {
                var coinName = symbol["instId"].ToString().Split('-');

                var pair = new TradingPair(coinName[0], coinName[1]);
                if (TradingPair.IsSupportedPair(pair))
                    coinNames.Add(pair);
            }

            return coinNames;
        }
    }
}
