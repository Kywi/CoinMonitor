using System;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;
using Newtonsoft.Json;
using CoinMonitor.WebSockets;

namespace CoinMonitor.Connections.Binance
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.Binance _binance;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public Connection()
        {
            _websocket = new Manager("wss://stream.binance.com:9443/ws", false);
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _websocket.OnConnected += WebsocketOnOnConnected;
            _binance = new Crypto.Exchange.Binance();
        }

        public async Task StartAsync()
        {
            await _websocket.Start();
        }

        public IExchange GetExchange()
        {
            return _binance;
        }

        private async void WebsocketOnOnConnected(object sender, EventArgs e)
        {
            var requestParams = _binance.SupportedPairs.Select(pair => $"{pair.Base.ToLower()}{pair.Quote.ToLower()}@ticker").ToList();

            var subscription = new WebSocketSubscriptionDto
            {
                Method = "SUBSCRIBE",
                Params = requestParams,
                Id = 1
            };

            await _websocket.Send(JsonConvert.SerializeObject(subscription));
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

            if (update?.Symbol == null)
                return;

            var coinName = update.Symbol.Substring(0, update.Symbol.Length - 4);
            PriceUpdate?.Invoke(this, new PriceChangedEventArgs(coinName, update.Price, "Binance"));
        }
    }
}