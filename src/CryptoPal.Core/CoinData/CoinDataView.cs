namespace CryptoPal.Core.CoinData;

/// <summary>Detailed coin metadata and per-currency market snapshots.</summary>
/// <param name="Id">CoinGecko coin identifier.</param>
/// <param name="Symbol">Ticker symbol.</param>
/// <param name="Name">Display name.</param>
/// <param name="Description">English description.</param>
/// <param name="ImageUrl">URL of the preferred coin image.</param>
/// <param name="PriceChangePercentage24h">24-hour price change percentage.</param>
/// <param name="MarketSnapshots">Current price, market cap, and volume per currency.</param>
public record CoinDataView(
    string Id,
    string Symbol,
    string Name,
    string Description,
    string ImageUrl,
    decimal PriceChangePercentage24h,
    IReadOnlyList<CoinMarketSnapshot> MarketSnapshots);
