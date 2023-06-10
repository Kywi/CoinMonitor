using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.Concurrent;
using CoinMonitor.Connections;
using CoinMonitor.Models;

namespace CoinMonitor
{
    public sealed partial class MainPage : Page
    {
        public ObservableConcurrentDictionary<string, Coin> Coins = new ObservableConcurrentDictionary<string, Coin>();

        private readonly ConnectionsManager _connectionsManager;

        public MainPage()
        {
            this.InitializeComponent();

            Coins["BTC"] = new Coin("BTC");
            Coins["ETH"] = new Coin("ETH");
            _connectionsManager = new ConnectionsManager(PriceUpdate);
        }

        private void PriceUpdate(object sender, PriceChangedEventArgs e)
        {
            if (Coins.TryGetValue(e.Symbol, out var coin))
            {
                var bidAsk = coin.CoinPrices[e.ExchangeName];
                if (e.Ask.HasValue)
                    bidAsk.Ask = e.Ask.Value;
                if (e.Bid.HasValue)
                    bidAsk.Bid = e.Bid.Value;

                coin.CoinPrices[e.ExchangeName] = bidAsk;
            }
            else
            {
                var newCoin = new Coin(e.Symbol);
                Coins[e.Symbol] = newCoin;
                var bidAsk = newCoin.CoinPrices[e.ExchangeName];
                if (e.Ask.HasValue)
                    bidAsk.Ask = e.Ask.Value;
                if (e.Bid.HasValue)
                    bidAsk.Bid = e.Bid.Value;

                newCoin.CoinPrices[e.ExchangeName] = bidAsk;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _connectionsManager.Connect();
        }
    }
}
