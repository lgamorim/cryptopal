namespace CryptoPal.Core.HistoricalMarketData;

public class HistoricalMarketDataView
{
    public required string Coin { get; init; }
    public required string Currency { get; init; }
    public required IList<DatedValue> Prices { get; init; }
    public required IList<DatedValue> MarketCaps { get; init; }
    public required IList<DatedValue> TotalVolumes { get; init; }
}
