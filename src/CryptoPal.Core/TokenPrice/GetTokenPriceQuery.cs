namespace CryptoPal.Core.TokenPrice;

/// <summary>Parameters for querying token prices by contract address.</summary>
public class GetTokenPriceQuery
{
    /// <summary>CoinGecko asset platform identifier.</summary>
    public required string AssetPlatformId { get; init; }

    /// <summary>Token contract addresses on the platform.</summary>
    public required IEnumerable<string> ContractAddresses { get; init; }

    /// <summary>Target currency codes.</summary>
    public required IEnumerable<string> Currencies { get; init; }
}
