using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoinMonitor.Connections.Models;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.Utils;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoinMonitor.Connections.KuCoin
{
    public class Connection : IConnectionManager
    {
        private List<Manager> _websockets;
        private List<Task> _receiveTasks;
        private List<List<WebSocketSubscription>> _subscriptions;
        private SemaphoreLocker _semaphore;
        private readonly Dictionary<string, BidAsk> _coinNameBidAskPrices;
        private readonly Crypto.Exchange.KuCoin _kuCoin;
        private int _subscriptionIndex = 0;

        public Connection()
        {
            _coinNameBidAskPrices = new Dictionary<string, BidAsk>();
            _semaphore = new SemaphoreLocker();
            _subscriptions = new List<List<WebSocketSubscription>>();
            _websockets = new List<Manager>();
            _receiveTasks = new List<Task>();
            _kuCoin = new Crypto.Exchange.KuCoin();
        }

        public void Dispose()
        {
            foreach (var websocket in _websockets)
                websocket.Dispose();
        }

        public string GetName()
        {
            return Crypto.Exchange.KuCoin.GetName();
        }

        public async Task<Dictionary<string, BidAsk>> GetCoinNameBidAskPrices()
        {
            return await _semaphore.LockAsync(() =>
            {
                var coinNameBidAskPrices = new Dictionary<string, BidAsk>(_coinNameBidAskPrices);
                return Task.FromResult(coinNameBidAskPrices);
            });
        }

        public async Task StartAsync()
        {
            var coinPairs = Utils.CollectionsHelpers.SplitList(_kuCoin.SupportedPairs.Select(pair => $"{pair.Base}-{pair.Quote}").ToList(), 99);

            var client = new HttpClient();

            var response = await client.PostAsync("https://api.kucoin.com/api/v1/bullet-public", null);

            var data = JObject.Parse(await response.Content.ReadAsStringAsync())["data"];

            var token = data["token"].ToString();
            var serverInstance = (JObject)((JArray)data["instanceServers"])[0];

            var endpoint = serverInstance["endpoint"].ToString();
            var pingInterval = serverInstance["pingInterval"].ToObject<double>();
            var pingMessage = JsonConvert.SerializeObject(new { id = Guid.NewGuid().ToString(), type = "ping" });

            _websockets.Add(InitWebsocket(endpoint, token, pingMessage, pingInterval));
            _subscriptions.Add(new List<WebSocketSubscription>());
            var id = 228;
            var subscriptionCount = 0;
            foreach (var coinPair in coinPairs)
            {
                id++;

                subscriptionCount += coinPair.Count;

                if (subscriptionCount >= 299)
                {
                    _subscriptions.Add(new List<WebSocketSubscription>());
                    _websockets.Add(InitWebsocket(endpoint, token, pingMessage, pingInterval));
                    subscriptionCount = coinPair.Count;
                }

                var topic = coinPair.Aggregate("/spotMarket/level2Depth5:", (current, pair) => current + pair + ",");
                topic = topic.Substring(0, topic.Length - 1);
                var subscription = new WebSocketSubscription
                {
                    Id = id,
                    Type = "subscribe",
                    Topic = topic,
                    IsPrivateChannel = false,
                    IsResponse = false
                };

                _subscriptions[0].Add(subscription);
            }

            foreach (var websocket in _websockets)
                _receiveTasks.Add(Task.Run(websocket.Start));
            await Task.WhenAll(_receiveTasks);
        }

        public IExchange GetExchange()
        {
            return _kuCoin;
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

            if (update?.DataDto == null)
                return;

            var coinName = update.Topic.Split(':')[1].Split('-')[0];
            decimal? bid = null;
            decimal? ask = null;
            if (update.DataDto.Ask != null)
                ask = update.DataDto.Ask[0][0];
            if (update.DataDto.Bid != null)
                bid = update.DataDto.Bid[0][0];

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

        private Manager InitWebsocket(string endpoint, string token, string pingMessage, double pingInterval)
        {
            var url = endpoint + "?token=" + token + "&connectId=" + Guid.NewGuid();
            var manager = new Manager(url, true, pingMessage, pingInterval);
            manager.MessageReceived += WebsocketOnMessageReceived;
            manager.OnConnected += ManagerOnOnConnected;
            return manager;
        }

        private async void ManagerOnOnConnected(object sender, EventArgs e)
        {
            var subscription = await _semaphore.LockAsync(() =>
            {
                if (_subscriptionIndex >= _subscriptions.Count)
                    _subscriptionIndex = 0;
                return Task.FromResult(_subscriptions[_subscriptionIndex++]);
            });

            var socket = sender as Manager;
            if (socket == null)
                return;

            foreach (var webSocketSubscription in subscription)
            {
                Thread.Sleep(250);
                await socket.Send(JsonConvert.SerializeObject(webSocketSubscription));
            }
        }
    }
}
