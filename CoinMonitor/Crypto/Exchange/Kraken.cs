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

        public List<string> SupportedCoins { get; private set; }

        public Kraken()
        {
            _url = "https://api.kraken.com/0/public/AssetPairs";
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
            var coins = JsonConvert.DeserializeObject<Dictionary<string, CoinDto>>(JObject.Parse(content)["result"].ToString());

            var coinNames = new HashSet<string>();
            foreach (var coin in coins)
            {
                var names = coin.Value.WsName.Split('/');

                if (names[1] == "USDT" /*|| names[1] == "USD"*/)
                    coinNames.Add(names[0]);
            }

            coinNames.Remove("USDT");
            return coinNames;
        }
    }
}