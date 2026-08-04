namespace CryptoPal.Core.HistoricalMarketData;

/// <summary>Historical price, market cap, and volume series for a coin.</summary>
/// <param name="Coin">CoinGecko coin identifier.</param>
/// <param name="Currency">Quote currency code.</param>
/// <param name="Prices">Daily closing prices.</param>
/// <param name="MarketCaps">Daily market capitalizations.</param>
/// <param name="TotalVolumes">Daily trading volumes.</param>
public record HistoricalMarketDataView(
    string Coin,
    string Currency,
    IReadOnlyList<DatedValue> Prices,
    IReadOnlyList<DatedValue> MarketCaps,
    IReadOnlyList<DatedValue> TotalVolumes);
