﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoinMonitor.Crypto.Exchange;
using CoinMonitor.Utils;
using CoinMonitor.WebSockets;
using Newtonsoft.Json;

namespace CoinMonitor.Connections.Bybit
{
    public class Connection : IConnectionManager
    {
        private readonly Manager _websocket;
        private readonly Crypto.Exchange.Bybit _bybit;

        public event EventHandler<PriceChangedEventArgs> PriceUpdate;

        public Connection()
        {
            _websocket = new Manager("wss://stream.bybit.com/v5/public/spot", pingMessage: JsonConvert.SerializeObject(new { op = "ping" }));
            _websocket.MessageReceived += WebsocketOnMessageReceived;
            _bybit = new Crypto.Exchange.Bybit();
        }

        public async Task StartAsync()
        {
            var paramsForRequests = SupprortedCoins.SplitList(_bybit.SupportedCoins.Select(symbol => $"tickers.{symbol.ToUpper()}USDT").ToList(), 10);

            await _websocket.Connect();
            foreach (var paramsForRequest in paramsForRequests)
            {
                var subscription = new WebSocketSubscriptionDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Operation = "subscribe",
                    Parameters = paramsForRequest.ToArray()
                };
                Thread.Sleep(250); // Don`t send to many request for single connection at small period of time
                await _websocket.Send(JsonConvert.SerializeObject(subscription));
            }

            await _websocket.StartReceiving();
        }

        public IExchange GetExchange()
        {
            return _bybit;
        }

        private void WebsocketOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            TickerDto update;
            try
            {
                update = JsonConvert.DeserializeObject<TickerDto>(e.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            if (update?.Data == null)
                return;

            var tradingPair = update.Data.TradingPair;
            var coinName = tradingPair.Substring(0, tradingPair.Length - 4);
            PriceUpdate?.Invoke(this, new PriceChangedEventArgs(coinName, Convert.ToDecimal(update.Data.ClosePrice), "Bybit"));
        }
    }
}