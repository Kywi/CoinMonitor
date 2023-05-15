using System;
using System.Threading.Tasks;

namespace CoinMonitor.WebSockets
{
    public class Manager
    {
        private readonly WebSocketConnection _connection;
        private readonly Pinger _pinger;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public Manager(string url, double pingInterval, string pingMessage)
        {
            _connection = new WebSocketConnection(url);
            _pinger = new Pinger(_connection, pingInterval, pingMessage);
        }

        public async Task Connect()
        {
            await _connection.Connect();
            _pinger.Start();
        }

        public async Task Close()
        {
            await _connection.Close();
        }

        public async Task Send(string text)
        {
            await _connection.Send(text);
        }

        public async Task StartReceiving()
        {
            while (_connection.IsOpen())
            {
                var message = await _connection.ReceiveMessage();
                if (message != null)
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
            }
        }
    }
}