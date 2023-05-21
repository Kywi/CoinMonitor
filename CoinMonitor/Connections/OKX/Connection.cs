using System;
using System.Linq;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.OKX
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.OKX _okx;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public Connection()
        {
            _websocket = new Manager("wss://ws.okx.com:8443/ws/v5/public", false);
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _okx = new Crypto.Exchange.OKX();
        }

        public async Task StartAsync()
        {
            var coins = _okx.SupportedPairs.Select(pair => $"{pair.Base}-{pair.Quote}").ToList();
            var args = new ArgsDto[coins.Count];
            var i = 0;
            foreach (var coin in coins)
            {
                args.SetValue(new ArgsDto { Channel = "tickers", InstId = coin }, i++);
            }

            await _websocket.Connect();
            var subscription = new WebSocketSubscriptionDto
            {
                Operation = "subscribe",
                Parameters = args,

            };

            await _websocket.Send(JsonConvert.SerializeObject(subscription));

            await _websocket.StartReceiving();
        }

        public IExchange GetExchange()
        {
            return _okx;
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

            var tradingPair = update.Data[0].Symbol;
            var coinName = tradingPair.Substring(0, tradingPair.Length - 5);
            PriceUpdate?.Invoke(this, new PriceChangedEventArgs(coinName, Convert.ToDecimal(update.Data[0].Price), "OKX"));
        }
    }
}