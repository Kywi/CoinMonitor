using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoinMonitor.Models
{
    public class Coin : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name = "";
        private double _priceBinance = 0.0;
        private double _priceWhiteBit = 0.0;

        public Coin(string name, double priceBinance, double priceWhiteBit)
        {
            Name = name;
            PriceBinance = priceBinance;
            PriceWhiteBit = priceWhiteBit;
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public double PriceBinance
        {
            get => _priceBinance;
            set => SetField(ref _priceBinance, value);
        }

        public double PriceWhiteBit
        {
            get => _priceWhiteBit;
            set => SetField(ref _priceWhiteBit, value);
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