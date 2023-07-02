using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public class Kraken : IExchange
    {
        class CoinDto
        {
            [JsonProperty("wsname")]
            public string WsName { get; set; }
        }

        private readonly string _url;

        public List<TradingPair> SupportedPairs { get; private set; }

        public Kraken()
        {
            _url = "https://api.kraken.com/0/public/AssetPairs";
        }

        public static string GetName()
        {
            return "Kraken";
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

            var coins = JsonConvert.DeserializeObject<Dictionary<string, CoinDto>>(JObject.Parse(content)["result"].ToString());
            var coinNames = new HashSet<TradingPair>();
            foreach (var coin in coins)
            {
                var names = coin.Value.WsName.Split('/');
                var pair = new TradingPair(names[0], names[1]);
                if (pair.Base != "USDT" && pair.Base != "USD" && pair.Base != "EUR" && pair.Quote is "USDT" or "USD")
                    coinNames.Add(pair);
            }

            return coinNames;
        }
    }
}