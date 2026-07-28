namespace CryptoPal.Core.CoinData;

/// <summary>Market figures for a coin in a single quote currency.</summary>
/// <param name="Currency">Quote currency code.</param>
/// <param name="CurrentPrice">Latest price.</param>
/// <param name="MarketCap">Market capitalization.</param>
/// <param name="TotalVolume">24-hour trading volume.</param>
public record CoinMarketSnapshot(string Currency, decimal CurrentPrice, decimal MarketCap, decimal TotalVolume);
