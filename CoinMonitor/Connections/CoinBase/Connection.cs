using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoinMonitor.Connections.CoinBase
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.CoinBase _coinBase;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;
        public Connection()
        {
            _websocket = new Manager("wss://ws-feed.exchange.coinbase.com", false);
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _websocket.OnConnected += WebsocketOnOnConnected;
            _coinBase = new Crypto.Exchange.CoinBase();
        }

        public async Task StartAsync()
        {
            await _websocket.Start();
        }

        public IExchange GetExchange()
        {
            return _coinBase;
        }

        private async void WebsocketOnOnConnected(object sender, EventArgs e)
        {
            var productIds = _coinBase.SupportedPairs.Select(pair => $"{pair.Base}-{pair.Quote}").ToList();

            var subscription = new WebSocketSubscription
            {
                Type = "subscribe",
                ProductIds = productIds,
                Channels = new List<string> { "ticker" }
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

            if (update?.ProductId == null)
                return;

            var coinName = update.ProductId.Substring(0, update.ProductId.Length - 5);
            PriceUpdate?.Invoke(this, new PriceChangedEventArgs(coinName, update.Price, "CoinBase"));
        }
    }
}
