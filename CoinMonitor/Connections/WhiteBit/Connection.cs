using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoinMonitor.Connections.WhiteBit
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.WhiteBit _whiteBit;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;
        public Connection()
        {
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

        public async Task StartAsync()
        {
            await _websocket.Start();
        }

        public IExchange GetExchange()
        {
            return _whiteBit;
        }

        private async void WebsocketOnOnConnected(object sender, EventArgs e)
        {
            //   var requestParams = _whiteBit.SupportedPairs.Select(pair => $"{pair.Base}_{pair.Quote}").ToList();
            var symbols = new List<string> { "BTC_USDT"};
            List<object> r = symbols.Cast<object>().ToList();
            r.Add(1);
            r.Add("0");
            r.Add(true);
            var subscription = new WebSocketSubscription
            {
                Method = "depth_subscribe",
                Params = r,
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

            if (update?.Params == null)
                return;

            var coinName = update.Params[0].Substring(0, update.Params[0].Length - 5);
            // PriceUpdate?.Invoke(this, new PriceChangedEventArgs(coinName, decimal.Parse(update.Params[1], NumberStyles.Float), "WhiteBit"));
        }
    }
}
