using System.Text.Json.Serialization;
using CryptoPal.ApiClient.CoinGecko;

namespace CryptoPal.ApiClient.CoinGecko.CoinData;

public class CoinDataResponse : IApiResponse
{
    public bool HasRequestSucceeded { get; init; }
    public required CoinDetail Coin { get; init; }

    public class CoinDetail
    {
        [JsonPropertyName("id")] public string? Id { get; init; }

        [JsonPropertyName("symbol")] public string? Symbol { get; init; }

        [JsonPropertyName("name")] public string? Name { get; init; }

        [JsonPropertyName("description")] public IDictionary<string, string>? Description { get; init; }

        [JsonPropertyName("image")] public CoinImage? Image { get; init; }

        [JsonPropertyName("market_data")] public CoinMarketData? MarketData { get; init; }
    }

    public class CoinImage
    {
        [JsonPropertyName("thumb")] public string? Thumb { get; init; }

        [JsonPropertyName("small")] public string? Small { get; init; }

        [JsonPropertyName("large")] public string? Large { get; init; }
    }

    public class CoinMarketData
    {
        [JsonPropertyName("current_price")] public IDictionary<string, decimal>? CurrentPrice { get; init; }

        [JsonPropertyName("market_cap")] public IDictionary<string, decimal>? MarketCap { get; init; }

        [JsonPropertyName("total_volume")] public IDictionary<string, decimal>? TotalVolume { get; init; }

        [JsonPropertyName("price_change_percentage_24h")] public decimal? PriceChangePercentage24h { get; init; }
    }
}
