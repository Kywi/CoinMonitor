using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.Concurrent;
using CoinMonitor.Models;

namespace CoinMonitor
{
    public sealed partial class MainPage : Page
    {
        private CoinPriceModel _coinPriceModel = new CoinPriceModel();

        public ObservableConcurrentDictionary<string, Coin> Coins = new ObservableConcurrentDictionary<string, Coin>();

        private MessageWebSocket _webSocket;
        public MainPage()
        {
            this.InitializeComponent();
            Coins["BTC"] = new Coin("BTC", 0, 0);
            Coins["ETH"] = new Coin("ETH", 0, 0);
            Coins["ALPHA"] = new Coin("ALPHA", 0, 0);

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _webSocket = new MessageWebSocket();
            _webSocket.Control.MessageType = SocketMessageType.Utf8;
            _webSocket.MessageReceived += WebSocketOnMessageReceived;
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var json = new JsonObject
            {
                { "id", JsonValue.CreateNumberValue(1) },
                { "method", JsonValue.CreateStringValue("SUBSCRIBE") },
                {"params", new JsonArray
                {
                    JsonValue.CreateStringValue("btcusdt@trade"),
                    JsonValue.CreateStringValue("ethusdt@trade"),
                    JsonValue.CreateStringValue("alphausdt@trade")
                }}
            };
            var connectTask = _webSocket.ConnectAsync(new Uri("wss://stream.binance.com:9443/ws")).AsTask();
            await connectTask.ContinueWith(_ => this.SendMessageUsingMessageWebSocketAsync(json.ToString()));

        }

        private async Task SendMessageUsingMessageWebSocketAsync(string message)
        {
            using (var dataWriter = new DataWriter(_webSocket.OutputStream))
            {
                dataWriter.WriteString(message);
                await dataWriter.StoreAsync();
                dataWriter.DetachStream();
            }
            Debug.WriteLine("Sending message using MessageWebSocket: " + message);
        }

        private async void WebSocketOnMessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (var dataReader = args.GetDataReader())
                {
                    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    var message = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                    var jsonResponse = JsonObject.Parse(message);
                    if (jsonResponse.ContainsKey("result") && jsonResponse.ContainsKey("id"))
                    {
                        return;
                    }

                    Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                         () =>
                    {
                        var coinName = jsonResponse["s"].GetString();
                        Coins[coinName.Substring(0, coinName.Length - 4)].PriceBinance = Convert.ToDouble(jsonResponse["p"].GetString());
                    }
                     );
                }
            }
            catch (Exception ex)
            {
                var webErrorStatus = WebSocketError.GetStatus(ex.GetBaseException().HResult);
            }

        }

    }
}
