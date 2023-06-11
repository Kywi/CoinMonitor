using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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
        private void WebsocketOnMessageReceived(object sender, MessageReceivedEventArgs e)
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

            if (update?.Params == null)
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
            PriceUpdate?.Invoke(this, new PriceChangedEventArgs(coinName, bid, ask, "WhiteBit"));
        }
    }
}
