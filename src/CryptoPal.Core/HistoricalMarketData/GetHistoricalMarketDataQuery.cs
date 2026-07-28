namespace CryptoPal.Core.HistoricalMarketData;

/// <summary>Parameters for querying historical market data.</summary>
public class GetHistoricalMarketDataQuery
{
    /// <summary>CoinGecko coin identifier.</summary>
    public required string Coin { get; init; }

    /// <summary>Quote currency code.</summary>
    public required string Currency { get; init; }

    /// <summary>Number of days of history to retrieve.</summary>
    public int Days { get; init; }
}
