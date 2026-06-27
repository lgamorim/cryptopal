using System.Text.Json.Serialization;
using CryptoPal.ApiClient.CoinGecko;

namespace CryptoPal.ApiClient.CoinGecko.CoinHistory;

public class CoinHistoryResponse : IApiResponse
{
    public bool HasRequestSucceeded { get; init; }
    public required CoinHistoryDetail Coin { get; init; }

    public class CoinHistoryDetail
    {
        [JsonPropertyName("id")] public string? Id { get; init; }

        [JsonPropertyName("symbol")] public string? Symbol { get; init; }

        [JsonPropertyName("name")] public string? Name { get; init; }

        [JsonPropertyName("image")] public CoinImage? Image { get; init; }

        [JsonPropertyName("market_data")] public CoinMarketData? MarketData { get; init; }

        [JsonPropertyName("developer_data")] public CoinDeveloperData? DeveloperData { get; init; }
    }

    public class CoinImage
    {
        [JsonPropertyName("thumb")] public string? Thumb { get; init; }

        [JsonPropertyName("small")] public string? Small { get; init; }
    }

    public class CoinMarketData
    {
        [JsonPropertyName("current_price")] public IDictionary<string, decimal>? CurrentPrice { get; init; }

        [JsonPropertyName("market_cap")] public IDictionary<string, decimal>? MarketCap { get; init; }

        [JsonPropertyName("total_volume")] public IDictionary<string, decimal>? TotalVolume { get; init; }
    }

    public class CoinDeveloperData
    {
        [JsonPropertyName("forks")] public int? Forks { get; init; }

        [JsonPropertyName("stars")] public int? Stars { get; init; }

        [JsonPropertyName("subscribers")] public int? Subscribers { get; init; }

        [JsonPropertyName("total_issues")] public int? TotalIssues { get; init; }

        [JsonPropertyName("closed_issues")] public int? ClosedIssues { get; init; }

        [JsonPropertyName("pull_requests_merged")] public int? PullRequestsMerged { get; init; }

        [JsonPropertyName("pull_request_contributors")] public int? PullRequestContributors { get; init; }

        [JsonPropertyName("code_additions_deletions_4_weeks")] public CodeChanges? CodeAdditionsDeletions4Weeks { get; init; }

        [JsonPropertyName("commit_count_4_weeks")] public int? CommitCount4Weeks { get; init; }
    }

    public class CodeChanges
    {
        [JsonPropertyName("additions")] public int? Additions { get; init; }

        [JsonPropertyName("deletions")] public int? Deletions { get; init; }
    }
}
