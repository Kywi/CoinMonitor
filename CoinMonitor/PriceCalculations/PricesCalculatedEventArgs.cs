using System;
using System.Collections.Generic;
using CoinMonitor.PriceCalculations.Models;

namespace CoinMonitor.PriceCalculations;

public class PricesCalculatedEventArgs : EventArgs
{
    public Dictionary<string, Coin> CoinNameBidAskPrices;

    public PricesCalculatedEventArgs(Dictionary<string, Coin> coinNameBidAskPrices)
    {
        CoinNameBidAskPrices = coinNameBidAskPrices;
    }
}