using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoinMonitor.Connections.Kraken;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoinMonitor.Connections.KuCoin
{
    public class Connection : IConnectionManager
    {
        private List<Manager> _websockets;
        private List<Task> _receiveTasks;
        private readonly Crypto.Exchange.KuCoin _kuCoin;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;
        public Connection()
        {
            _websockets = new List<Manager>();
            _receiveTasks = new List<Task>();
            _kuCoin = new Crypto.Exchange.KuCoin();
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


            _websockets.Add(await InitWebsocket(endpoint, token, pingMessage, pingInterval));
            int id = 228;
            int subscriptionCount = 0;
            foreach (var coinPair in coinPairs)
            {
                id++;

                subscriptionCount += coinPair.Count;

                if (subscriptionCount >= 299)
                {
                    _websockets.Add(await InitWebsocket(endpoint, token, pingMessage, pingInterval));
                    subscriptionCount = coinPair.Count;
                }

                var topic = coinPair.Aggregate("/market/ticker:", (current, pair) => current + pair + ",");
                topic = topic.Substring(0, topic.Length - 1);
                var subscription = new WebSocketSubscription
                {
                    Id = id,
                    Type = "subscribe",
                    Topic = topic,
                    IsPrivateChannel = false,
                    IsResponse = false
                };
                Thread.Sleep(250);
                await _websockets.Last().Send(JsonConvert.SerializeObject(subscription));
            }

            foreach (var websocket in _websockets)
                _receiveTasks.Add(websocket.StartReceiving());

            await Task.WhenAll(_receiveTasks);
        }

        public IExchange GetExchange()
        {
            return _kuCoin;
        }

        private void WebsocketOnMessageReceived(object sender, MessageReceivedEventArgs e)
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

            var coinName = update.Topic.Split(':')[1].Split('-')[0];

            PriceUpdate?.Invoke(this, new PriceChangedEventArgs(coinName, update.Data.Price, "KuCoin"));
        }

        private async Task<Manager> InitWebsocket(string endpoint, string token, string pingMessage, double pingInterval)
        {
            var url = endpoint + "?token=" + token + "&connectId=" + Guid.NewGuid();
            var manager = new Manager(url, true, pingMessage, pingInterval);
            manager.MessageReceived += WebsocketOnMessageReceived;
            await manager.Connect();
            return manager;
        }
    }
}
