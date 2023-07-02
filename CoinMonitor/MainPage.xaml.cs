using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.Concurrent;
using Windows.UI.Core;
using CoinMonitor.Connections;
using CoinMonitor.Models;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml.Media;
using CoinMonitor.PriceCalculations;
using Windows.UI.ViewManagement;
using System;
using System.Collections.Generic;

namespace CoinMonitor
{
    public sealed partial class MainPage : Page
    {
        public ObservableConcurrentDictionary<string, Coin> Coins;

        private readonly ConnectionsManager _connectionsManager;

        public MainPage()
        {
            InitializeComponent();
            Coins = new ObservableConcurrentDictionary<string, Coin>
            {
                ["BTC"] = new("BTC"),
                ["ETH"] = new("ETH")
            };
            _connectionsManager = new ConnectionsManager(PriceCalculated);
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
        }

        private async void PriceCalculated(object sender, PricesCalculatedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Bind(UpdateUICollection, e.CoinNameBidAskPrices));
        }

        private void UpdateUICollection(Dictionary<string, PriceCalculations.Models.Coin> newCoinNameBidAskPrices)
        {
            foreach (var coinNameBidAskPrices in newCoinNameBidAskPrices)
            {
                if (Coins.TryGetValue(coinNameBidAskPrices.Key, out var coinValue))
                {
                    coinValue.Name = coinNameBidAskPrices.Key;
                    foreach (var exchangeNameBidAskPrices in coinNameBidAskPrices.Value.ExchangeNameBidAsk)
                    {
                        var bidAskValue = coinValue.CoinsPricesView[exchangeNameBidAskPrices.Key];
                        bidAskValue.Ask = exchangeNameBidAskPrices.Value.Ask;
                        bidAskValue.Bid = exchangeNameBidAskPrices.Value.Bid;
                        bidAskValue.AskBrush = new SolidColorBrush(exchangeNameBidAskPrices.Value.AskColor) { Opacity = exchangeNameBidAskPrices.Value.AskOpacity };
                        bidAskValue.BidBrush = new SolidColorBrush(exchangeNameBidAskPrices.Value.BidColor) { Opacity = exchangeNameBidAskPrices.Value.BidOpacity };
                    }
                }
                else
                {
                    coinValue = new Coin(coinNameBidAskPrices.Value.Name);
                    foreach (var exchangeNameBidAskPrices in coinNameBidAskPrices.Value.ExchangeNameBidAsk)
                    {
                        var bidAskValue = coinValue.CoinsPricesView[exchangeNameBidAskPrices.Key];
                        bidAskValue.Ask = exchangeNameBidAskPrices.Value.Ask;
                        bidAskValue.Bid = exchangeNameBidAskPrices.Value.Bid;
                        bidAskValue.AskBrush = new SolidColorBrush(exchangeNameBidAskPrices.Value.AskColor) { Opacity = exchangeNameBidAskPrices.Value.AskOpacity }; ;
                        bidAskValue.BidBrush = new SolidColorBrush(exchangeNameBidAskPrices.Value.BidColor) { Opacity = exchangeNameBidAskPrices.Value.BidOpacity };
                    }
                    Coins[coinNameBidAskPrices.Key] = coinValue;
                }
            }

        }

        public DispatchedHandler Bind<T>(Action<T> func, T arg)
        {
            return () => func(arg);
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            _connectionsManager.Dispose();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _connectionsManager.Connect();
        }
    }
}
