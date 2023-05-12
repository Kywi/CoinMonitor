using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CoinMonitor.Utils;
using System.IO;

namespace CoinMonitor.Connections.Binance
{
    public class BinanceWebSocketManager : IWebSocketManager
    {
        private readonly ClientWebSocket _socket;
        private readonly string _baseUrl;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public BinanceWebSocketManager()
        {
            _socket = new ClientWebSocket();
            _baseUrl = "wss://stream.binance.com:9443/ws";
        }

        public async Task StartAsync()
        {
            var requestParams = (await SupprortedCoins.GetSupportedCoinsForBinance()).Select(symbol => $"{symbol.ToLower()}usdt@ticker").ToList();

            await _socket.ConnectAsync(new Uri(_baseUrl), CancellationToken.None);

            var subscription = new WebSocketSubscriptionDto
            {
                Method = "SUBSCRIBE",
                Params = requestParams,
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
            while (_socket.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    var messageBuffer = WebSocket.CreateClientBuffer(1024 * 4, 16);
                    result = await _socket.ReceiveAsync(messageBuffer, CancellationToken.None);
                    ms.Write(messageBuffer.Array, messageBuffer.Offset, result.Count);
                } while (!result.EndOfMessage);
                    
                if (result.MessageType == WebSocketMessageType.Close)
                    await StopAsync();
                else
                {
                    if (!result.EndOfMessage)
                        continue;

                    var json = Encoding.UTF8.GetString(ms.ToArray());
                    TickerDto update;
                    try
                    {
                        update = JsonConvert.DeserializeObject<TickerDto>(json);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        continue;
                    }

                    if (update?.Symbol != null)
                        PriceUpdate?.Invoke(this,
                            new PriceChangedEventArgs(update.Symbol.Substring(0, update.Symbol.Length - 4),
                                update.Price, "Binance"));
                }
            }
        }
    }
}