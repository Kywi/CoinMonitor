using System;
using System.Timers;

namespace CoinMonitor.WebSockets
{
    public class Pinger : IDisposable
    {
        private readonly WebSocketConnection _connection;
        private readonly string _pingMessage;
        private readonly Timer _timer;

        public Pinger(WebSocketConnection connection, double interval, string pingMessage)
        {
            _connection = connection;
            _pingMessage = pingMessage;
            _timer = new Timer
            {
                AutoReset = true,
                Interval = interval,
            };
            _timer.Elapsed += TimerOnElapsed;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public void Start()
        {
            _timer.Start();
        }

        private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            await _connection.Send(_pingMessage);
        }
    }
}