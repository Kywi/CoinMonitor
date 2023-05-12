using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoinMonitor.Utils
{
    public class SupprortedCoins
    {
        public static async Task<List<string>> GetSupportedCoinsForBinance()
        {
            var client = new HttpClient();

            var response = await client.GetAsync("https://api.binance.com/api/v3/exchangeInfo");

            var content = await response.Content.ReadAsStringAsync();

            var exchangeInfo = JObject.Parse(content);
            var symbols = (JArray)exchangeInfo["symbols"];

            var coinNames = new HashSet<string>();
            foreach (var symbol in symbols)
            {
                var parts = symbol["baseAsset"].ToString();
                coinNames.Add(parts.ToLower());
            }

            coinNames.Remove("USDT");
            return coinNames.ToList();
        }

        public static async Task<List<string>> GetSupportedCoinsForWhiteBit()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://whitebit.com/api/v4/public/markets");

            var content = await response.Content.ReadAsStringAsync();
            var markets = JArray.Parse(content);

            var coinNames = new HashSet<string>();
            foreach (var market in markets)
            {
                var name = market["name"].ToString();
                var names = name.Split('_');
                if (names[1] == "USDT")
                    coinNames.Add(names[0]);
            }

            coinNames.Remove("USDT");
            return coinNames.ToList();
        }

        public static async Task<List<string>> GetSupportedCoinsForByBit()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://api.bybit.com/v5/market/tickers?category=spot");

            var content = await response.Content.ReadAsStringAsync();
            JObject symbols = JObject.Parse(content);
            JArray result = (JArray)symbols["result"]["list"];

            var coinNames = new HashSet<string>();
            foreach (var symbol in result)
            {
                var coinName = symbol["symbol"].ToString();
                if (coinName.Contains("USDT"))
                    coinNames.Add(coinName.Substring(0, coinName.Length - 4));
            }

            coinNames.Remove("USD");
            coinNames.Remove("USDT");

            return coinNames.ToList();
        }

        public static List<List<string>> SplitList(List<string> requestParams, int chunkSize = 30)
        {
            var list = new List<List<string>>();

            for (var i = 0; i < requestParams.Count; i += chunkSize)
                list.Add(requestParams.GetRange(i, Math.Min(chunkSize, requestParams.Count - i)));

            return list;
        }
    }
}