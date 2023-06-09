﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoinMonitor.Crypto.Exchange
{
    public class Binance : IExchange
    {
        private readonly string _url;

        public List<TradingPair> SupportedPairs { get; private set; }

        public Binance()
        {
            SupportedPairs = new List<TradingPair>();
            _url = "https://api.binance.com/api/v3/exchangeInfo";
        }

        public static string GetName()
        {
            return "Binance";
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

            var exchangeInfo = JObject.Parse(content);
            var symbols = (JArray)exchangeInfo["symbols"];
            var coinNames = new HashSet<TradingPair>();
            foreach (var symbol in symbols)
            {
                var parts = symbol["baseAsset"].ToString().ToUpper();
                var quoteAsset = symbol["quoteAsset"].ToString().ToUpper();
                var pair = new TradingPair(parts, quoteAsset);
                if (TradingPair.IsSupportedPair(pair))
                    coinNames.Add(pair);
            }

            return coinNames;
        }
    }
}