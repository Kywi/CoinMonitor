using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoinMonitor
{
    public class CoinPriceModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string _price { get; set; }

        public string Price
        {
            get { return _price; }
            set
            {
                _price = value;
                this.OnPropertyChanged();
            }

        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}