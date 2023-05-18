using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public class WhiteBit : IExchange
    {
        private readonly string _url;

        public List<string> SupportedCoins { get; private set; }

        public WhiteBit()
        {
            SupportedCoins = new List<string>();
            _url = "https://whitebit.com/api/v4/public/markets";
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
            return coinNames;
        }
    }
}