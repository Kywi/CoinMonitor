using System;
using CoinMonitor.Connections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CoinMonitor.PriceCalculations.Models;
using Windows.UI.Xaml.Media;

namespace CoinMonitor.PriceCalculations;

public class PriceCalculator : IDisposable
{
    private const int PollPricesIntervalMSec = 2000;
    private const int InitPollDelayMSec = 1000 * 10;
    private readonly System.Timers.Timer _timer;
    private readonly List<IConnectionManager> _connections;

    public event EventHandler<PricesCalculatedEventArgs> PriceCalculated;

    public PriceCalculator(List<IConnectionManager> connections)
    {
        _connections = connections;
        _timer = new System.Timers.Timer
        {
            AutoReset = false,
            Interval = PollPricesIntervalMSec,
        };
        _timer.Elapsed += TimerOnElapsed;
    }
    public void Dispose()
    {
        _timer?.Dispose();
    }

    public void Start()
    {
        _ = Task.Run(() =>
        {
            Thread.Sleep(InitPollDelayMSec);
            _timer.Start();
        });
    }

    private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        var coinNameBidAskPrices = await Calculate();
        PriceCalculated?.Invoke(this, new PricesCalculatedEventArgs(coinNameBidAskPrices));
        _timer.Start();
    }

    private async Task<Dictionary<string, Coin>> Calculate()
    {
        var coinNameBidAskPrices = new Dictionary<string, Coin>();
        foreach (var connection in _connections)
            InitCoinDictionaryWithConnection(connection.GetName(), await connection.GetCoinNameBidAskPrices(), coinNameBidAskPrices);

        foreach (var coin in coinNameBidAskPrices)
        {
            if (!coin.Value.ExchangeNameBidAsk.TryGetValue("Binance", out var mainCoinBidPrice))
                continue;

            foreach (var exchangeName in coin.Value.ExchangeNameBidAsk.Keys.ToList().Where(exchangeName => exchangeName != "Binance"))
            {
                var bindAsk = coin.Value.ExchangeNameBidAsk[exchangeName];
                coin.Value.ExchangeNameBidAsk[exchangeName] = CalculateBidAsk(mainCoinBidPrice, bindAsk);
            }
        }

        return coinNameBidAskPrices;
    }
    private static void InitCoinDictionaryWithConnection(string exchangeName, Dictionary<string, Connections.Models.BidAsk> coinNameBidAskPrices, IDictionary<string, Coin> coinDictionary)
    {
        foreach (var coinNameBidAskPrice in coinNameBidAskPrices)
        {
            if (coinDictionary.TryGetValue(coinNameBidAskPrice.Key, out var coin))
            {
                coin.ExchangeNameBidAsk[exchangeName] =
                    new BidAsk(coinNameBidAskPrice.Value.Bid, coinNameBidAskPrice.Value.Ask);
            }
            else
            {
                coin = new Coin(coinNameBidAskPrice.Key)
                {
                    ExchangeNameBidAsk =
                    {
                        [exchangeName] = new BidAsk(coinNameBidAskPrice.Value.Bid, coinNameBidAskPrice.Value.Ask)
                    }
                };
                coinDictionary[coinNameBidAskPrice.Key] = coin;
            }
        }
    }

    private BidAsk CalculateBidAsk(BidAsk main, BidAsk depend)
    {
        var result = new BidAsk(CalculatePercentage(main.Bid, depend.Bid), CalculatePercentage(main.Ask, depend.Ask));
        result.AskColor = CalculateColor(result.Ask);
        result.AskOpacity = CalculateOpacity(result.Ask);
        result.BidColor = CalculateColor(result.Bid);
        result.AskOpacity = CalculateOpacity(result.Bid);

        return result;
    }

    private static decimal CalculatePercentage(decimal mainPrice, decimal dependPrice)
    {
        if (mainPrice == 0 || dependPrice == 0)
            return 0;

        var result = Math.Round(dependPrice * 100 / mainPrice - 100, 2);

        return result;
    }

    private static double CalculateOpacity(decimal price)
    {
        var opacity = Convert.ToDouble(Math.Abs(price)) / 0.3 * 0.15;
        return opacity;
    }

    private static Windows.UI.Color CalculateColor(decimal price)
    {
        Windows.UI.Color resultColor;
        if (price > 0)
            resultColor = Windows.UI.Colors.Green;
        else if (price == 0)
            resultColor = Windows.UI.Colors.White;
        else
            resultColor = Windows.UI.Colors.Red;

        return resultColor;
    }

}