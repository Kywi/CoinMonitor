using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;

namespace CoinMonitor.Models;

public class BidAsk : INotifyPropertyChanged
{
    private decimal _bid;
    private decimal _ask;
    private Brush _bidBrush;
    private Brush _askBrush;

    public event PropertyChangedEventHandler PropertyChanged;

    public BidAsk()
    {
        _bid = 0;
        _ask = 0;
        _askBrush = new SolidColorBrush(Windows.UI.Colors.White);
        _bidBrush = new SolidColorBrush(Windows.UI.Colors.White);
    }

    public decimal Bid
    {
        get => _bid;
        set => SetField(ref _bid, value);
    }

    public decimal Ask
    {
        get => _ask;
        set => SetField(ref _ask, value);
    }

    public Brush BidBrush
    {
        get => _bidBrush;
        set => SetField(ref _bidBrush, value);
    }

    public Brush AskBrush
    {
        get => _askBrush;
        set => SetField(ref _askBrush, value);
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