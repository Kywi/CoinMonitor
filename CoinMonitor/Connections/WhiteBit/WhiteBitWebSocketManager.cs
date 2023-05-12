using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoinMonitor.Utils;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.WhiteBit
{
    public class WhiteBitWebSocketManager : IWebSocketManager
    {
        private readonly ClientWebSocket _socket;
        private readonly string _baseUrl;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public WhiteBitWebSocketManager()
        {
            _socket = new ClientWebSocket();
            _baseUrl = "wss://api.whitebit.com/ws";
        }

        public async Task StartAsync()
        {
            await _socket.ConnectAsync(new Uri(_baseUrl), CancellationToken.None);

            var subscription = new WebSocketSubscription
            {
                Method = "lastprice_subscribe",
                Params = (await SupprortedCoins.GetSupportedCoinsForWhiteBit()).Select(symbol => $"{symbol.ToUpper()}_USDT").ToList(),
                Id = 1
            };

            var json = JsonConvert.SerializeObject(subscription);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            Thread.Sleep(200);
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

                    if (update?.Params == null)
                        continue;

                    var coinName = update.Params[0].Substring(0, update.Params[0].Length - 5);
                    PriceUpdate?.Invoke(this,
                        new PriceChangedEventArgs(coinName, decimal.Parse(update.Params[1], NumberStyles.Float), "WhiteBit"));
                }
            }
        }
    }
}
