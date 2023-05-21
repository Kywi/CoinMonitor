using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoinMonitor.Connections.Kraken
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.Kraken _kraken;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;
        public Connection()
        {
            var pingMessage = new JObject
            {
                { "event", "ping" },
            };
            _websocket = new Manager("wss://ws.kraken.com", pingMessage: pingMessage.ToString());
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _kraken = new Crypto.Exchange.Kraken();
        }

        public async Task StartAsync()
        {
            var requestParams = _kraken.SupportedCoins.Select(pair => $"{pair.Base}/{pair.Quote}").ToList();

            await _websocket.Connect();
            var subscription = new WebSocketSubscription
            {
                Event = "subscribe",
                Pairs = requestParams,
                Subscription = new Subscription { Name = "ticker" }
            };
            await _websocket.Send(JsonConvert.SerializeObject(subscription));

            await _websocket.StartReceiving();
        }

        public IExchange GetExchange()
        {
            return _kraken;
        }

        private void WebsocketOnMessageReceived(object sender, MessageReceivedEventArgs e)
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

            if (update?.Price == null)
                return;

            coinName = coinName.Split('/')[0];
            PriceUpdate?.Invoke(this, new PriceChangedEventArgs(coinName, update.Price[0], "Kraken"));
        }
    }
}
