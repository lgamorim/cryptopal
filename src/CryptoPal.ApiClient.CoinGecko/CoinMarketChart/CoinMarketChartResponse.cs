using System.Text.Json.Serialization;
using CryptoPal.ApiClient.CoinGecko;

namespace CryptoPal.ApiClient.CoinGecko.CoinMarketChart;

public class CoinMarketChartResponse : IApiResponse
{
    public bool HasRequestSucceeded { get; init; }
    public required MarketChart HistoricalMarketData { get; init; }

    public class MarketChart
    {
        [JsonPropertyName("prices")] public required IList<MarketDataPoint> Prices { get; init; }

        [JsonPropertyName("market_caps")] public required IList<MarketDataPoint> MarketCaps { get; init; }

        [JsonPropertyName("total_volumes")] public required IList<MarketDataPoint> TotalVolumes { get; init; }
    }
}
