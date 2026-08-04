namespace CryptoPal.Core.CurrentPrice;

/// <summary>Result of a current-price query.</summary>
/// <param name="CoinPrices">Prices for each requested coin.</param>
public record CurrentPriceView(IEnumerable<CoinPrice> CoinPrices);
