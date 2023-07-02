using System.Runtime.InteropServices.ComTypes;
using Windows.UI.Xaml.Media;

namespace CoinMonitor.PriceCalculations.Models;

public struct BidAsk
{
    public decimal Bid;
    public decimal Ask;
    public Windows.UI.Color BidColor;
    public Windows.UI.Color AskColor;
    public double BidOpacity;
    public double AskOpacity;
    
    public BidAsk(decimal bid, decimal ask)
    {
        Bid = bid;
        Ask = ask;
        BidColor = Windows.UI.Colors.White;
        AskColor = Windows.UI.Colors.White;
        BidOpacity = 1;
        AskOpacity = 1;
    }
}