namespace CryptoPal.ApiClient.CoinGecko.SimpleTokenPrice;

public class SimpleTokenPriceRequest
{
    public required string AssetPlatformId { get; init; }
    public required IEnumerable<string> ContractAddresses { get; init; }
    public required IEnumerable<string> Currencies { get; init; }
}
