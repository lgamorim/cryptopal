namespace CryptoPal.Application.CurrentPrice;

public class CurrentPriceView
{
    public required IEnumerable<CoinPrice> CoinPrices { get; init; }
}
