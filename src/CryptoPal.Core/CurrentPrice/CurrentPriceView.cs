namespace CryptoPal.Core.CurrentPrice;

/// <summary>Result of a current-price query.</summary>
public class CurrentPriceView
{
    /// <summary>Prices for each requested coin.</summary>
    public required IEnumerable<CoinPrice> CoinPrices { get; init; }
}
