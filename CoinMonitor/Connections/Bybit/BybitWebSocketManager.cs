using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CoinMonitor.Utils;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.Bybit
{
    public class BybitWebSocketManager : IWebSocketManager
    {
        private readonly ClientWebSocket _socket;
        private readonly string _baseUrl;
        private readonly System.Timers.Timer _timer;
        private bool _needPing = false;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public BybitWebSocketManager()
        {
            _timer = new System.Timers.Timer(20000);
            _timer.AutoReset = true;
            _timer.Elapsed += TimerOnElapsed;
            _socket = new ClientWebSocket();
            _baseUrl = "wss://stream.bybit.com/v5/public/spot";
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _needPing = true;
        }

        public async Task StartAsync()
        {
            var paramsForRequests = SupprortedCoins.SplitList((await SupprortedCoins.GetSupportedCoinsForByBit()).Select(symbol => $"tickers.{symbol.ToUpper()}USDT").ToList(), 10);

            await _socket.ConnectAsync(new Uri(_baseUrl), CancellationToken.None);
            foreach (var paramsForRequest in paramsForRequests)
            {
                var subscription = new WebSocketSubscriptionDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Operation = "subscribe",
                    Parameters = paramsForRequest.ToArray(),
                };

                var json = JsonConvert.SerializeObject(subscription);
                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
                Thread.Sleep(200);
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
            _timer.Start();
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

                if (_needPing)
                    SendPing();

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

                    if (update?.Data == null)
                        continue;

                    var tradingPair = update.Data.TradingPair;
                    var coinName = tradingPair.Substring(0, tradingPair.Length - 4);
                    PriceUpdate?.Invoke(this,
                        new PriceChangedEventArgs(coinName, Convert.ToDecimal(update.Data.ClosePrice), "Bybit"));
                }
            }
        }

        private async void SendPing()
        {
            _needPing = false;

            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                op = "ping",
            })));

            await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}