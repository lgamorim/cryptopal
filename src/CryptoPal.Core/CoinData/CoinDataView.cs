namespace CryptoPal.Core.CoinData;

/// <summary>Detailed coin metadata and per-currency market snapshots.</summary>
public class CoinDataView
{
    /// <summary>CoinGecko coin identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Ticker symbol.</summary>
    public required string Symbol { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>English description.</summary>
    public required string Description { get; init; }

    /// <summary>URL of the preferred coin image.</summary>
    public required string ImageUrl { get; init; }

    /// <summary>24-hour price change percentage.</summary>
    public required decimal PriceChangePercentage24h { get; init; }

    /// <summary>Current price, market cap, and volume per currency.</summary>
    public required IReadOnlyList<CoinMarketSnapshot> MarketSnapshots { get; init; }
}
