using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Connections.Models;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.Utils;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoinMonitor.Connections.Kraken
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.Kraken _kraken;
        private readonly SemaphoreLocker _semaphore;
        private readonly Dictionary<string, BidAsk> _coinNameBidAskPrices;

        public Connection()
        {
            _semaphore = new SemaphoreLocker();
            _coinNameBidAskPrices = new Dictionary<string, BidAsk>();
            var pingMessage = new JObject
            {
                { "event", "ping" },
            };
            _websocket = new Manager("wss://ws.kraken.com", pingMessage: pingMessage.ToString());
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _websocket.OnConnected += WebsocketOnOnConnected;
            _kraken = new Crypto.Exchange.Kraken();
        }

        public void Dispose()
        {
            _websocket.Dispose();
        }

        public string GetName()
        {
            return Crypto.Exchange.Kraken.GetName();
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
            var requestParams = _kraken.SupportedPairs.Select(pair => $"{pair.Base}/{pair.Quote}").ToList();

            var subscription = new WebSocketSubscription
            {
                Event = "subscribe",
                Pairs = requestParams,
                Subscription = new Subscription { Name = "book", Depth = 10 }
            };
            await _websocket.Send(JsonConvert.SerializeObject(subscription));
        }

        public async Task StartAsync()
        {
            await _websocket.Start();
        }

        public IExchange GetExchange()
        {
            return _kraken;
        }

        private async void WebsocketOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message[0] == '{')
                return;

            TickerDto update;
            string coinName;
            try
            {
                var jsonArray = JArray.Parse(e.Message);
                if (jsonArray?.Count != 4)
                    return;

                coinName = jsonArray[3].ToString();
                update = JsonConvert.DeserializeObject<TickerDto>(jsonArray[1].ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            decimal? bid = null;
            decimal? ask = null;
            if (update.Ask != null)
                ask = update.Ask[0][0];
            if (update.Bid != null)
                bid = update.Bid[0][0];

            coinName = coinName.Split('/')[0];
            await _semaphore.LockAsync(() =>
            {
                if (_coinNameBidAskPrices.TryGetValue(coinName, out var bidAskValue))
                {
                    if (ask.HasValue)
                        bidAskValue.Ask = ask.Value;
                    if (bid.HasValue)
                        bidAskValue.Bid = bid.Value;
                }
                else
                {
                    bidAskValue = new BidAsk();
                    if (ask.HasValue)
                        bidAskValue.Ask = ask.Value;
                    if (bid.HasValue)
                        bidAskValue.Bid = bid.Value;
                    _coinNameBidAskPrices[coinName] = bidAskValue;
                }
                return Task.FromResult(0);
            });
        }
    }
}
