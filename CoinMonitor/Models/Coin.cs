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
        private ObservableStringDictionary<decimal> _coinsPricesView = new ObservableStringDictionary<decimal>();
        private ObservableStringDictionary<decimal> _coinsPrices = new ObservableStringDictionary<decimal>();
        private ObservableStringDictionary<Brush> _colors = new ObservableStringDictionary<Brush>();

        public event PropertyChangedEventHandler PropertyChanged;

        public Coin(string name)
        {
            Name = name;
            foreach (var exchange in Crypto.Manager.SupportedExchanges)
            {
                _coinsPrices[exchange] = 0;
                _coinsPricesView[exchange] = 0;
                _colors[exchange] = new SolidColorBrush(Windows.UI.Colors.White); ;
            }

            _coinsPrices.DictionaryChanged += CoinsPricesOnDictionaryChanged;
        }

        private void CoinsPricesOnDictionaryChanged(object sender, NotifyDictionaryChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Reset)
                return;

            if ((string)e.AddedKey == "Binance")
            {
                _coinsPricesView["Binance"] = (decimal)e.AddedItem;

                foreach (var coin in _coinsPricesView.Keys.ToList().Where(coin => coin != "Binance"))
                    _coinsPricesView[coin] = CalculatePercentage((decimal)e.AddedItem, _coinsPrices[coin], coin);
            }
            else
                _coinsPricesView[(string)e.AddedKey] = CalculatePercentage(_coinsPrices["Binance"], (decimal)e.AddedItem, (string)e.AddedKey);
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public ObservableStringDictionary<decimal> CoinPrices
        {
            get => _coinsPrices;
            set => SetField(ref _coinsPrices, value);
        }

        public ObservableStringDictionary<decimal> CoinsPricesView
        {
            get => _coinsPricesView;
            set => SetField(ref _coinsPricesView, value);
        }

        public ObservableStringDictionary<Brush> Colors
        {
            get => _colors;
            set => SetField(ref _colors, value);
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

        private decimal CalculatePercentage(decimal price, decimal coin, string exchangeName)
        {
            if (price == 0 || coin == 0) return 0;
            var result = Math.Round((coin * 100 / price) - 100, 2);

            var brush = new SolidColorBrush
            {
                Opacity = Convert.ToDouble(Math.Abs(result)) / 0.5
            };
            if (result > 0)
                brush.Color = Windows.UI.Colors.Green;
            else if (result == 0)
                brush.Color = Windows.UI.Colors.White;
            else
                brush.Color = Windows.UI.Colors.Red;

            _colors[exchangeName] = brush;

            return result;
        }
    }
}