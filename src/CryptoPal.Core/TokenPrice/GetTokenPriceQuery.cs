namespace CryptoPal.Core.TokenPrice;

public class GetTokenPriceQuery
{
    public required string AssetPlatformId { get; init; }
    public required IEnumerable<string> ContractAddresses { get; init; }
    public required IEnumerable<string> Currencies { get; init; }
}
