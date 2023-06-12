using System;
using System.Threading.Tasks;

namespace CoinMonitor.WebSockets
{
    public class Manager : IDisposable
    {
        private readonly string _url;
        private readonly bool _ifPingerEnabled;
        private readonly string _pingMessage;
        private readonly double _pingInterval;

        private Pinger _pinger;
        private WebSocketConnection _connection;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<EventArgs> OnConnected;

        public Manager(string url, bool ifPingerEnabled = true, string pingMessage = "", double pingInterval = 20000)
        {
            _url = url;
            _pingMessage = pingMessage;
            _pingInterval = pingInterval;
            _ifPingerEnabled = ifPingerEnabled;
        }

        public void Dispose()
        {
            if (_pinger != null)
            {
                _pinger.Dispose();
                _pinger = null;
            }
            _connection.Dispose();
            _connection = null;
        }

        public async Task Close()
        {
            await _connection.Close();
        }

        public async Task Send(string text)
        {
            await _connection.Send(text);
        }

        public async Task Start()
        {
            while (true)
            {
                try
                {
                    Init();
                    await Connect();
                    OnConnected?.Invoke(this, EventArgs.Empty);
                    await StartReceiving();
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Dispose();
                }
            }
        }

        private void Init()
        {
            _connection = new WebSocketConnection(_url);
            if (_ifPingerEnabled)
                _pinger = new Pinger(_connection, _pingInterval, _pingMessage);
        }

        private async Task Connect()
        {
            await _connection.Connect();
            _pinger?.Start();
        }

        private async Task StartReceiving()
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