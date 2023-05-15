using System;

namespace CoinMonitor.WebSockets
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageReceivedEventArgs(string message)
        {
            Message = message;
        }

    }
}