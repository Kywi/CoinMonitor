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
        private string _name = "";
        private ObservableStringDictionary<BidAsk> _coinsPricesView = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public Coin(string name)
        {
            Name = name;
            foreach (var exchange in Crypto.Manager.SupportedExchanges)
                _coinsPricesView[exchange] = new BidAsk();
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
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
    }
}