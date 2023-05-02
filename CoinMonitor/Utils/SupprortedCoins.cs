using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace CoinMonitor.Utils
{
    public class SupprortedCoins
    {
        public static List<string> GetSupportedCoins()
        {
            return new List<string>
            {
                "btcusdt",
                "ethusdt",
                "alphausdt"
            };
        }
    }
}