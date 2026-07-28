namespace CryptoPal.Core.CoinData;

/// <summary>Parameters for querying detailed coin metadata.</summary>
public class GetCoinDataQuery
{
    /// <summary>CoinGecko coin identifier.</summary>
    public required string Coin { get; init; }
}
