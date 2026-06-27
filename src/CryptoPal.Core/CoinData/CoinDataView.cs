namespace CryptoPal.Core.CoinData;

public class CoinDataView
{
    public required string Id { get; init; }
    public required string Symbol { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string ImageUrl { get; init; }
    public required decimal PriceChangePercentage24h { get; init; }
    public required IReadOnlyList<CoinMarketSnapshot> MarketSnapshots { get; init; }
}
