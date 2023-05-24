using System;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoinMonitor.Utils;

namespace CoinMonitor.WebSockets
{
    public class WebSocketConnection : IDisposable
    {
        private readonly string _baseUrl;
        private readonly SemaphoreLocker _semaphore;
        private ClientWebSocket _socket;

        public WebSocketConnection(string baseUrl)
        {
            _semaphore = new SemaphoreLocker();
            _socket = new ClientWebSocket();
            _baseUrl = baseUrl;
        }

        public void Dispose()
        {
            _socket.Dispose();
            _socket = null;
        }

        public async Task Connect()
        {
            await _socket.ConnectAsync(new Uri(_baseUrl), CancellationToken.None);
        }

        public async Task Close()
        {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by the client", CancellationToken.None);
        }

        public async Task Send(string text)
        {
            if (!IsOpen())
                return;

            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(text));
            await _semaphore.LockAsync(async () => await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None));
        }

        public bool IsOpen()
        {
            return _socket.State == WebSocketState.Open;
        }

        public async Task<string> ReceiveMessage()
        {
            while (IsOpen())
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    var messageBuffer = WebSocket.CreateClientBuffer(1024 * 4, 16);
                    result = await _semaphore.LockAsync(async () => await _socket.ReceiveAsync(messageBuffer, CancellationToken.None));
                    if (messageBuffer.Array != null)
                        ms.Write(messageBuffer.Array, messageBuffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                    await Close();
                else
                {
                    if (!result.EndOfMessage)
                        continue;

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }

            return "";
        }
    }
}