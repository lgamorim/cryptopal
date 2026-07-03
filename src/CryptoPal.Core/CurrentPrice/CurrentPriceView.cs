namespace CryptoPal.Core.CurrentPrice;

public class CurrentPriceView
{
    public required IEnumerable<CoinPrice> CoinPrices { get; init; }
}
