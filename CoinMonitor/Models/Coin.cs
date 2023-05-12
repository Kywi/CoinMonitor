using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;
using ObservableDictionary;

namespace CoinMonitor.Models
{
    public class Coin : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name = "";

        private ObservableStringDictionary<decimal> _coinsPricesView = new ObservableStringDictionary<decimal>();

        private ObservableStringDictionary<decimal> _coinsPrices = new ObservableStringDictionary<decimal>();
        private ObservableStringDictionary<Brush> _colors = new ObservableStringDictionary<Brush>();

        public Coin(string name)
        {
            Name = name;
            _coinsPrices["Binance"] = 0;
            _coinsPrices["WhiteBit"] = 0;
            _coinsPrices["Bybit"] = 0;

            _coinsPricesView["Binance"] = 0;
            _coinsPricesView["WhiteBit"] = 0;
            _coinsPricesView["Bybit"] = 0;

            _colors["Binance"] = new SolidColorBrush(Windows.UI.Colors.White);
            _colors["WhiteBit"] = new SolidColorBrush(Windows.UI.Colors.White);
            _colors["Bybit"] = new SolidColorBrush(Windows.UI.Colors.White);

            _coinsPrices.DictionaryChanged += CoinsPricesOnDictionaryChanged;
        }

        private void CoinsPricesOnDictionaryChanged(object sender, NotifyDictionaryChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if ((string)e.AddedKey == "Binance")
                {
                    _coinsPricesView["Binance"] = (decimal)e.AddedItem;

                    foreach (var coin in _coinsPricesView.Keys.ToList())
                    {
                        if (coin != "Binance")
                            _coinsPricesView[coin] = CalculatePercentage((decimal)e.AddedItem, _coinsPrices[coin], coin);
                    }
                }
                else
                    _coinsPricesView[(string)e.AddedKey] = CalculatePercentage(_coinsPrices["Binance"], (decimal)e.AddedItem, (string)e.AddedKey);
            }
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

            if (result > 0)
                _colors[exchangeName] = new SolidColorBrush(Windows.UI.Colors.Green);
            else if( result == 0)
                _colors[exchangeName] = new SolidColorBrush(Windows.UI.Colors.White);
            else
                _colors[exchangeName] = new SolidColorBrush(Windows.UI.Colors.Red);

            return result;
        }
    }
}