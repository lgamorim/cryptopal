namespace CryptoPal.Core.CurrentPrice;

/// <summary>Current prices for a single coin across multiple currencies.</summary>
/// <param name="Id">CoinGecko coin identifier.</param>
/// <param name="Prices">Latest prices per currency code.</param>
public record CoinPrice(string Id, IEnumerable<Price> Prices);
