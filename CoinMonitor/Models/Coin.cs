using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ObservableDictionary;

namespace CoinMonitor.Models
{
    public class Coin : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name = "";

        private ObservableStringDictionary<decimal> _coinsPrices = new ObservableStringDictionary<decimal>();

        public Coin(string name)
        {
            Name = name;
            _coinsPrices["Binance"] = 0;
            _coinsPrices["WhiteBit"] = 0;
            _coinsPrices["Bybit"] = 0;
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