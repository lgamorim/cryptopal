namespace CryptoPal.Core.DeveloperData;

/// <summary>Developer repository activity for a coin on a historical date.</summary>
/// <param name="Id">CoinGecko coin identifier.</param>
/// <param name="Symbol">Ticker symbol.</param>
/// <param name="Name">Display name.</param>
/// <param name="Forks">Repository fork count.</param>
/// <param name="Stars">Repository star count.</param>
/// <param name="Subscribers">Repository subscriber count.</param>
/// <param name="TotalIssues">Total open and closed issue count.</param>
/// <param name="ClosedIssues">Closed issue count.</param>
/// <param name="PullRequestsMerged">Merged pull request count.</param>
/// <param name="PullRequestContributors">Distinct pull request contributor count.</param>
/// <param name="CodeAdditions">Lines of code added in the last four weeks.</param>
/// <param name="CodeDeletions">Lines of code deleted in the last four weeks.</param>
/// <param name="CommitCount4Weeks">Commit count in the last four weeks.</param>
public record DeveloperDataView(
    string Id,
    string Symbol,
    string Name,
    int Forks,
    int Stars,
    int Subscribers,
    int TotalIssues,
    int ClosedIssues,
    int PullRequestsMerged,
    int PullRequestContributors,
    int CodeAdditions,
    int CodeDeletions,
    int CommitCount4Weeks);
