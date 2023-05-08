using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ObservableDictionary;

namespace CoinMonitor.Models
{
    public class Coin : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name = "";

        private ObservableStringDictionary<decimal> _coinsPricesView = new ObservableStringDictionary<decimal>();

        private ObservableStringDictionary<decimal> _coinsPrices = new ObservableStringDictionary<decimal>();

        public Coin(string name)
        {
            Name = name;
            _coinsPrices["Binance"] = 0;
            _coinsPrices["WhiteBit"] = 0;
            _coinsPrices["Bybit"] = 0;

            _coinsPricesView["Binance"] = 0;
            _coinsPricesView["WhiteBit"] = 0;
            _coinsPricesView["Bybit"] = 0;

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
                            _coinsPricesView[coin] = CalculatePercentage((decimal)e.AddedItem, _coinsPrices[coin]);
                    }
                }
                else
                    _coinsPricesView[(string)e.AddedKey] = CalculatePercentage(_coinsPrices["Binance"], (decimal)e.AddedItem);
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

        private static int CalculatePercentage(decimal price, decimal coin)
        {
            if (price == 0 || coin == 0) return 0;
            return (int)(coin * 100 / price) - 100;
        }
    }
}