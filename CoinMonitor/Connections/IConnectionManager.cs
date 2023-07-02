using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoinMonitor.Connections.Models;
using CoinMonitor.Crypto.Exchange;

namespace CoinMonitor.Connections
{
    public interface IConnectionManager : IDisposable
    {
        public Task StartAsync();
        public string GetName();
        public Task<Dictionary<string, BidAsk>> GetCoinNameBidAskPrices();
        public IExchange GetExchange();
    }
}
