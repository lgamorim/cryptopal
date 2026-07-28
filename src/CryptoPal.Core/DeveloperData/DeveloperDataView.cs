namespace CryptoPal.Core.DeveloperData;

/// <summary>Developer repository activity for a coin on a historical date.</summary>
public class DeveloperDataView
{
    /// <summary>CoinGecko coin identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Ticker symbol.</summary>
    public required string Symbol { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Repository fork count.</summary>
    public required int Forks { get; init; }

    /// <summary>Repository star count.</summary>
    public required int Stars { get; init; }

    /// <summary>Repository subscriber count.</summary>
    public required int Subscribers { get; init; }

    /// <summary>Total open and closed issue count.</summary>
    public required int TotalIssues { get; init; }

    /// <summary>Closed issue count.</summary>
    public required int ClosedIssues { get; init; }

    /// <summary>Merged pull request count.</summary>
    public required int PullRequestsMerged { get; init; }

    /// <summary>Distinct pull request contributor count.</summary>
    public required int PullRequestContributors { get; init; }

    /// <summary>Lines of code added in the last four weeks.</summary>
    public required int CodeAdditions { get; init; }

    /// <summary>Lines of code deleted in the last four weeks.</summary>
    public required int CodeDeletions { get; init; }

    /// <summary>Commit count in the last four weeks.</summary>
    public required int CommitCount4Weeks { get; init; }
}
