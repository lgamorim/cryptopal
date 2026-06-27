namespace CryptoPal.Core.DeveloperData;

public class DeveloperDataView
{
    public required string Id { get; init; }
    public required string Symbol { get; init; }
    public required string Name { get; init; }
    public required int Forks { get; init; }
    public required int Stars { get; init; }
    public required int Subscribers { get; init; }
    public required int TotalIssues { get; init; }
    public required int ClosedIssues { get; init; }
    public required int PullRequestsMerged { get; init; }
    public required int PullRequestContributors { get; init; }
    public required int CodeAdditions { get; init; }
    public required int CodeDeletions { get; init; }
    public required int CommitCount4Weeks { get; init; }
}
