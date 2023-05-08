using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoinMonitor.Connections.Bybit;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.Bybit
{
    public class BybitWebSocketManager : IWebSocketManager
    {
        private readonly ClientWebSocket _socket;
        private readonly string _baseUrl;
        private readonly List<string> _symbols;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public BybitWebSocketManager(List<string> symbols)
        {
            _socket = new ClientWebSocket();
            _baseUrl =
                "wss://stream.bybit.com/spot/public/v3"; //"wss://stream.bybit.com/v5/public/spot"; wss://stream.bybit.com/realtime
            _symbols = new List<string>();

            foreach (var symbol in symbols)
                _symbols.Add("tickers." + symbol.ToUpper() + "USDT");
        }


        public async Task StartAsync()
        {
            await _socket.ConnectAsync(new Uri(_baseUrl), CancellationToken.None);

            var subscription = new WebSocketSubscriptionDto
            {
                Id = 1,
                Operation = "subscribe",
                Params = _symbols,

            };

            var json = JsonConvert.SerializeObject(subscription);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

            await ReceiveAsync();
        }

        public async Task StopAsync()
        {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by the client",
                CancellationToken.None);
        }

        private async Task ReceiveAsync()
        {
            var buffer = new byte[1024 * 4];

            while (_socket.State == WebSocketState.Open)
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                    await StopAsync();
                else
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    TickerDto update;
                    try
                    {
                        update = JsonConvert.DeserializeObject<TickerDto>(json);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    if (update?.Data == null)
                        continue;

                    var tradingPair = update.Data.TradingPair;
                    var coinName = tradingPair.Substring(0, tradingPair.Length - 4);
                    PriceUpdate?.Invoke(this,
                        new PriceChangedEventArgs(coinName, Convert.ToDecimal(update.Data.OpenPrice), "Bybit"));
                }
            }
        }
    }
}