using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoinMonitor.Connections.Models;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.Utils;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.Bybit
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.Bybit _bybit;
        private readonly SemaphoreLocker _semaphore;
        private readonly Dictionary<string, BidAsk> _coinNameBidAskPrices;

        public Connection()
        {
            _semaphore = new SemaphoreLocker();
            _coinNameBidAskPrices = new Dictionary<string, BidAsk>();
            _websocket = new Manager("wss://stream.bybit.com/v5/public/spot", pingMessage: JsonConvert.SerializeObject(new { op = "ping" }), pingInterval: 10000);
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _websocket.OnConnected += WebsocketOnOnConnected;
            _bybit = new Crypto.Exchange.Bybit();
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
            return _bybit;
        }

        public string GetName()
        {
            return Crypto.Exchange.Bybit.GetName();
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
            var paramsForRequests = CollectionsHelpers.SplitList(_bybit.SupportedPairs.Select(pair => $"orderbook.1.{pair.Base}{pair.Quote}").ToList(), 10);

            foreach (var paramsForRequest in paramsForRequests)
            {
                var subscription = new WebSocketSubscriptionDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Operation = "subscribe",
                    Parameters = paramsForRequest.ToArray()
                };
                Thread.Sleep(250); // Don`t send to many request for single connection at small period of time
                await _websocket.Send(JsonConvert.SerializeObject(subscription));
            }
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

            var tradingPair = update.Data.TradingPair;
            var coinName = tradingPair.Substring(0, tradingPair.Length - 4);

            decimal? bid = null;
            decimal? ask = null;
            if (update.Data.Ask is { Count: > 0 })
                ask = update.Data.Ask[0][0];
            if (update.Data.Bid is { Count: > 0 })
                bid = update.Data.Bid[0][0];

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