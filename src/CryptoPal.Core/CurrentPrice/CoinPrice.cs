namespace CryptoPal.Core.CurrentPrice;

/// <summary>Current prices for a single coin across multiple currencies.</summary>
public class CoinPrice
{
    /// <summary>CoinGecko coin identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Latest prices per currency code.</summary>
    public required IEnumerable<Price> Prices { get; init; }
}
