using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;
using CoinMonitor.WebSockets;
using ObservableDictionary;

namespace CoinMonitor.Models
{
    public class Coin : INotifyPropertyChanged
    {
        private string _name = "";
        private ObservableStringDictionary<BidAsk> _coinsPricesView = new();
        private ObservableStringDictionary<BidAsk> _coinsPrices = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public Coin(string name)
        {
            Name = name;
            foreach (var exchange in Crypto.Manager.SupportedExchanges)
            {
                _coinsPrices[exchange] = new BidAsk();
                _coinsPricesView[exchange] = new BidAsk();
            }

            _coinsPrices.DictionaryChanged += CoinsPricesOnDictionaryChanged;
        }

        private void CoinsPricesOnDictionaryChanged(object sender, NotifyDictionaryChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Reset)
                return;

            var changedItem = (BidAsk)e.AddedItem;

            if ((string)e.AddedKey == "Binance")
            {
                _coinsPricesView["Binance"] = changedItem;

                foreach (var exchangeName in _coinsPricesView.Keys.ToList().Where(coin => coin != "Binance"))
                    CalculateBidAsk(_coinsPricesView[exchangeName], changedItem, _coinsPrices[exchangeName]);
            }
            else
                CalculateBidAsk(_coinsPricesView[(string)e.AddedKey], _coinsPrices["Binance"], changedItem);
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public ObservableStringDictionary<BidAsk> CoinPrices
        {
            get => _coinsPrices;
            set => SetField(ref _coinsPrices, value);
        }

        public ObservableStringDictionary<BidAsk> CoinsPricesView
        {
            get => _coinsPricesView;
            set => SetField(ref _coinsPricesView, value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void CalculateBidAsk(BidAsk viewValue, BidAsk main, BidAsk depend)
        {
            viewValue.Ask = CalculatePercentage(main.Ask, depend.Ask);
            viewValue.Bid = CalculatePercentage(main.Bid, depend.Bid);
            viewValue.AskBrush = CalculateBrush(viewValue.Ask);
            viewValue.BidBrush = CalculateBrush(viewValue.Bid);
        }

        private decimal CalculatePercentage(decimal price, decimal coin)
        {
            if (price == 0 || coin == 0)
                return 0;

            var result = Math.Round((coin * 100 / price) - 100, 2);

            return result;
        }

        private Brush CalculateBrush(decimal price)
        {
            var brush = new SolidColorBrush
            {
                Opacity = Convert.ToDouble(Math.Abs(price)) / 0.3 * 0.15
            };
            if (price > 0)
                brush.Color = Windows.UI.Colors.Green;
            else if (price == 0)
                brush.Color = Windows.UI.Colors.White;
            else
                brush.Color = Windows.UI.Colors.Red;

            return brush;
        }
    }
}