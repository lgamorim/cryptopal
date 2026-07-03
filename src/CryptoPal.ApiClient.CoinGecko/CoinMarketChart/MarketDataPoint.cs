using System.Text.Json.Serialization;

namespace CryptoPal.ApiClient.CoinGecko.CoinMarketChart;

[JsonConverter(typeof(MarketDataPointJsonConverter))]
public record MarketDataPoint(long TimestampMs, decimal Value);
