namespace CryptoPal.Core.HistoricalMarketData;

/// <summary>Historical price, market cap, and volume series for a coin.</summary>
public class HistoricalMarketDataView
{
    /// <summary>CoinGecko coin identifier.</summary>
    public required string Coin { get; init; }

    /// <summary>Quote currency code.</summary>
    public required string Currency { get; init; }

    /// <summary>Daily closing prices.</summary>
    public required IList<DatedValue> Prices { get; init; }

    /// <summary>Daily market capitalizations.</summary>
    public required IList<DatedValue> MarketCaps { get; init; }

    /// <summary>Daily trading volumes.</summary>
    public required IList<DatedValue> TotalVolumes { get; init; }
}
