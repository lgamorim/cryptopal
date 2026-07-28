namespace CryptoPal.Core.CurrentPrice;

/// <summary>Parameters for querying current coin prices.</summary>
public class GetCurrentPriceQuery
{
    /// <summary>CoinGecko coin identifiers to quote.</summary>
    public required IEnumerable<string> Coins { get; init; }

    /// <summary>Target currency codes.</summary>
    public required IEnumerable<string> Currencies { get; init; }
}
