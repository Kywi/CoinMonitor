using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CoinMonitor.Utils;
using CoinMonitor.WebSockets;
using Newtonsoft.Json.Linq;

namespace CoinMonitor.Connections.Binance
{
    public class Connection : IWebSocketManager
    {
        private readonly Manager _websocket;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public Connection()
        {
            var pingMessage = new JObject
            {
                { "id", Guid.NewGuid().ToString() },
                { "method", "ping" }
            };
            _websocket = new Manager("wss://stream.binance.com:9443/ws", 120000, pingMessage.ToString());
            _websocket.MessageReceived += WebsocketOnMessageReceived;
        }

        public async Task StartAsync()
        {
            var requestParams = (await SupprortedCoins.GetSupportedCoinsForBinance()).Select(symbol => $"{symbol.ToLower()}usdt@ticker").ToList();

            await _websocket.Connect();
            var subscription = new WebSocketSubscriptionDto
            {
                Method = "SUBSCRIBE",
                Params = requestParams,
                Id = 1
            };
            await _websocket.Send(JsonConvert.SerializeObject(subscription));

            await _websocket.StartReceiving();
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