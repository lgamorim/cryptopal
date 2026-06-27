namespace CryptoPal.Application.HistoricalMarketData;

public class GetHistoricalMarketDataQuery
{
    public required string Coin { get; init; }
    public required string Currency { get; init; }
    public int Days { get; init; }
}
