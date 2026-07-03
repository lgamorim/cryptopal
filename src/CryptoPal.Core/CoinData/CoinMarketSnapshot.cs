namespace CryptoPal.Core.CoinData;

public record CoinMarketSnapshot(string Currency, decimal CurrentPrice, decimal MarketCap, decimal TotalVolume);
