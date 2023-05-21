namespace CoinMonitor.Crypto.Exchange
{
    public readonly struct TradingPair
    {
        public string Base { get; }

        public string Quote { get; }

        public TradingPair(string @base, string quote)
        {
            Base = @base;
            Quote = quote;
        }

        public readonly override int GetHashCode()
        {
            return Base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = (TradingPair)obj;
            return Base == other.Base;
        }

        public static bool IsSupportedPair(TradingPair pair)
        {
            return pair.Base != "USDT" && pair.Base != "USD" && pair.Base != "EUR" && pair.Quote is "USDT";
        }
    }
}