using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoinMonitor.Connections.Models;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.Utils;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoinMonitor.Connections.WhiteBit
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.WhiteBit _whiteBit;
        private readonly SemaphoreLocker _semaphore;
        private readonly Dictionary<string, BidAsk> _coinNameBidAskPrices;

        public Connection()
        {
            _semaphore = new SemaphoreLocker();
            _coinNameBidAskPrices = new Dictionary<string, BidAsk>();
            var pingMessage = new JObject
            {
                { "id", 228 },
                { "method", "ping" },
                { "params", new JArray() }
            };
            _websocket = new Manager("wss://api.whitebit.com/ws", pingMessage: pingMessage.ToString());
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _websocket.OnConnected += WebsocketOnOnConnected;
            _whiteBit = new Crypto.Exchange.WhiteBit();
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
            return _whiteBit;
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
            var pairs = _whiteBit.SupportedPairs.Select(pair => $"{pair.Base}_{pair.Quote}").ToList();
            foreach (var pair in pairs)
            {
                var @params = new List<object> { pair, 1, "0", true };
                var subscription = new WebSocketSubscription
                {
                    Method = "depth_subscribe",
                    Params = @params,
                    Id = 1
                };
                await _websocket.Send(JsonConvert.SerializeObject(subscription));
            }
        }
        private async void WebsocketOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Contains("failed"))
                throw new Exception("Failed to subscribe message=" + e.Message);

            if (e.Message.Contains("result"))
                return;

            BidAskDto priceUpdate;
            TickerDto update;
            try
            {
                update = JsonConvert.DeserializeObject<TickerDto>(e.Message);
                priceUpdate = JsonConvert.DeserializeObject<BidAskDto>(update.Params[1].ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            if (update.Params == null)
                return;

            if (priceUpdate == null)
                return;

            decimal? bid = null;
            decimal? ask = null;
            if (priceUpdate.Ask != null)
                ask = decimal.Parse(priceUpdate.Ask[0][0], NumberStyles.Float);
            if (priceUpdate.Bid != null)
                bid = decimal.Parse(priceUpdate.Bid[0][0], NumberStyles.Float);

            var coinName = update.Params[2].ToString().Split("_")[0];
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
