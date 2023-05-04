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
using System.Collections.Generic;
using CoinMonitor.Connections;
using CoinMonitor.Models;
using CoinMonitor.Utils;

namespace CoinMonitor
{
    public sealed partial class MainPage : Page
    {
        public ObservableConcurrentDictionary<string, Coin> Coins = new ObservableConcurrentDictionary<string, Coin>();

        private readonly ConnectionsManager _connectionsManager;

        public MainPage()
        {
            this.InitializeComponent();

            _connectionsManager = new ConnectionsManager(SupprortedCoins.GetSupportedCoins(), PriceUpdate);
            foreach (var coinName in SupprortedCoins.GetSupportedCoins())
                Coins[coinName.ToUpper()] = new Coin(coinName.ToUpper());
        }

        private void PriceUpdate(object sender, PriceChangedEventArgs e)
        {
            Coins[e.Symbol].CoinPrices[e.ExcgangeName] = e.Price;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _connectionsManager.Connect();
        }
    }
}
