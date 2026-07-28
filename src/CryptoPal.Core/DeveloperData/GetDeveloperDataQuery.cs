namespace CryptoPal.Core.DeveloperData;

/// <summary>Parameters for querying developer activity on a historical date.</summary>
public class GetDeveloperDataQuery
{
    /// <summary>CoinGecko coin identifier.</summary>
    public required string Coin { get; init; }

    /// <summary>Snapshot date in <c>dd-mm-yyyy</c> format.</summary>
    public required string Date { get; init; }
}
