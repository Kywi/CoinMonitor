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
                coin.CoinPrices[e.ExchangeName] = e.Price;
            else
            {
                var newCoin = new Coin(e.Symbol);
                Coins[e.Symbol] = newCoin;
                newCoin.CoinPrices[e.ExchangeName] = e.Price;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _connectionsManager.Connect();
        }
    }
}
