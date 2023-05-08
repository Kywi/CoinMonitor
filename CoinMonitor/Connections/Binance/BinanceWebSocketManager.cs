using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CoinMonitor.Connections;
using CoinMonitor.Connections.Binance;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.Binance
{
    public class BinanceWebSocketManager : IWebSocketManager
    {
        private readonly ClientWebSocket _socket;
        private readonly string _baseUrl;
        private readonly List<string> _symbols;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public BinanceWebSocketManager(List<string> symbols)
        {
            _socket = new ClientWebSocket();
            _baseUrl = "wss://stream.binance.com:9443/ws";
            _symbols = new List<string>();

            foreach (var symbol in symbols)
                _symbols.Add(symbol + "usdt");
        }

        public async Task StartAsync()
        {
            await _socket.ConnectAsync(new Uri(_baseUrl), CancellationToken.None);

            foreach (var symbol in _symbols)
            {
                var subscription = new WebSocketSubscriptionDto
                {
                    Method = "SUBSCRIBE",
                    Params = new List<string> { $"{symbol.ToLower()}@ticker" },
                    Id = 1
                };

                var json = JsonConvert.SerializeObject(subscription);
                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
                await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }

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
                    var update = JsonConvert.DeserializeObject<TickerDto>(json);

                    if (update?.Symbol != null)
                        PriceUpdate?.Invoke(this, new PriceChangedEventArgs(update.Symbol.Substring(0, update.Symbol.Length - 4), update.Price, "Binance"));
                }
            }
        }
    }
}