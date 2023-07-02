using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CoinMonitor.Connections.Models;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.Utils;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.OKX
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.OKX _okx;
        private readonly SemaphoreLocker _semaphore;
        private readonly Dictionary<string, BidAsk> _coinNameBidAskPrices;

        public Connection()
        {
            _semaphore = new SemaphoreLocker();
            _coinNameBidAskPrices = new Dictionary<string, BidAsk>();
            _websocket = new Manager("wss://ws.okx.com:8443/ws/v5/public", false);
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _websocket.OnConnected += WebsocketOnOnConnected;
            _okx = new Crypto.Exchange.OKX();
        }

        public void Dispose()
        {
            _websocket.Dispose();
        }

        public async Task StartAsync()
        {
            await _websocket.Start();
        }

        public IExchange GetExchange()
        {
            return _okx;
        }
        public string GetName()
        {
            return Crypto.Exchange.OKX.GetName();
        }

        public async Task<Dictionary<string, BidAsk>> GetCoinNameBidAskPrices()
        {
            return await _semaphore.LockAsync(() =>
            {
                var coinNameBidAskPrices = new Dictionary<string, BidAsk>(_coinNameBidAskPrices);
                return Task.FromResult(coinNameBidAskPrices);
            });
        }

        private async void WebsocketOnOnConnected(object sender, EventArgs e)
        {
            var coins = _okx.SupportedPairs.Select(pair => $"{pair.Base}-{pair.Quote}").ToList();
            var args = new ArgsDto[coins.Count];
            var i = 0;
            foreach (var coin in coins)
            {
                args.SetValue(new ArgsDto { Channel = "tickers", InstId = coin }, i++);
            }

            var subscription = new WebSocketSubscriptionDto
            {
                Operation = "subscribe",
                Parameters = args,

            };

            await _websocket.Send(JsonConvert.SerializeObject(subscription));
        }

        private async void WebsocketOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            TickerDto update;
            try
            {
                update = JsonConvert.DeserializeObject<TickerDto>(e.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            if (update?.Data == null)
                return;

            var tradingPair = update.Data[0].Symbol;
            var coinName = tradingPair.Substring(0, tradingPair.Length - 5);
            await _semaphore.LockAsync(() =>
            {
                _coinNameBidAskPrices[coinName] = new BidAsk(update.Data[0].Bid, update.Data[0].Ask);
                return Task.FromResult(0);
            });
        }
    }
}