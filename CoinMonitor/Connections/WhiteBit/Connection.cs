using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Utils;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoinMonitor.Connections.WhiteBit
{
    public class Connection : IWebSocketManager
    {
        private readonly Manager _websocket;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;
        public Connection()
        {
            var pingMessage = new JObject
            {
                { "id", 228 },
                { "method", "ping" },
                { "params", new JArray() }
            };
            _websocket = new Manager("wss://api.whitebit.com/ws", 20000, pingMessage.ToString());
            _websocket.MessageReceived += WebsocketOnMessageReceived;
        }

        public async Task StartAsync()
        {
            var requestParams = (await SupprortedCoins.GetSupportedCoinsForWhiteBit()).Select(symbol => $"{symbol.ToUpper()}_USDT").ToList();

            await _websocket.Connect();
            var subscription = new WebSocketSubscription
            {
                Method = "lastprice_subscribe",
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

            if (update?.Params == null)
                return;

            var coinName = update.Params[0].Substring(0, update.Params[0].Length - 5);
            PriceUpdate?.Invoke(this, new PriceChangedEventArgs(coinName, decimal.Parse(update.Params[1], NumberStyles.Float), "WhiteBit"));
        }
    }
}
