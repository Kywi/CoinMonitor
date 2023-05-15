using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public class Binance : IExchange
    {
        private readonly string _url;
        
        public List<string> SupportedCoins { get; private set; }
        
        public Binance()
        {
            _url = "https://api.binance.com/api/v3/exchangeInfo";
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

            var exchangeInfo = JObject.Parse(content);
            var symbols = (JArray)exchangeInfo["symbols"];

            var coinNames = new HashSet<string>();
            foreach (var symbol in symbols)
            {
                var parts = symbol["baseAsset"].ToString();
                var quoteAsset = symbol["quoteAsset"].ToString();
                if(quoteAsset == "USDT")
                coinNames.Add(parts.ToUpper());
            }

            coinNames.Remove("USDT");
            return coinNames;
        }
    }
}