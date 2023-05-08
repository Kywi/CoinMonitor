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

namespace CoinMonitor.Connections.WhiteBit
{
    public class WhiteBitWebSocketManager : IWebSocketManager
    {
        private readonly ClientWebSocket _socket;
        private readonly string _baseUrl;
        private readonly List<string> _symbols;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public WhiteBitWebSocketManager(List<string> symbols)
        {
            _socket = new ClientWebSocket();
            _baseUrl = "wss://api.whitebit.com/ws";
            _symbols = new List<string>();

            foreach (var symbol in symbols)
                _symbols.Add(symbol.ToUpper() + "_USDT");
        }

        public async Task StartAsync()
        {
            await _socket.ConnectAsync(new Uri(_baseUrl), CancellationToken.None);

            var subscription = new WebSocketSubscription
            {
                Method = "lastprice_subscribe",
                Params = _symbols,
                Id = 1
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
                        continue;
                    }

                    if (update?.Params == null)
                        continue;

                    var coinName = update.Params[0].Substring(0, update.Params[0].Length - 5);
                    PriceUpdate?.Invoke(this,
                        new PriceChangedEventArgs(coinName, Convert.ToDecimal(update.Params[1]), "WhiteBit"));
                }
            }
        }
    }
}