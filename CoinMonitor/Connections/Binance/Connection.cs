using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;
using Newtonsoft.Json;
using CoinMonitor.WebSockets;
using CoinMonitor.Utils;
using System.Threading;
using CoinMonitor.Connections.Models;

namespace CoinMonitor.Connections.Binance
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.Binance _binance;
        private readonly SemaphoreLocker _semaphore;
        private readonly Dictionary<string, BidAsk> _coinNameBidAskPrices;

        public Connection()
        {
            _semaphore = new SemaphoreLocker();
            _websocket = new Manager("wss://stream.binance.com:9443/ws", false);
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _websocket.OnConnected += WebsocketOnOnConnected;
            _binance = new Crypto.Exchange.Binance();
            _coinNameBidAskPrices = new Dictionary<string, BidAsk>();
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
            return _binance;
        }

        public string GetName()
        {
            return Crypto.Exchange.Binance.GetName();
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
            var requestParams = _binance.SupportedPairs.Select(pair => $"{pair.Base.ToLower()}{pair.Quote.ToLower()}@bookTicker").ToList();
            var subscription = new WebSocketSubscriptionDto
            {
                Method = "SUBSCRIBE",
                Params = requestParams,
                Id = 1
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

            if (update?.Symbol == null)
                return;

            var coinName = update.Symbol.Substring(0, update.Symbol.Length - 4);

            await _semaphore.LockAsync(() =>
            {
                _coinNameBidAskPrices[coinName] = new BidAsk(update.Ask, update.Bid);
                return Task.FromResult(0);
            });
        }
    }
}