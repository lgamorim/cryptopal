namespace CryptoPal.ApiClient.CoinGecko;

/// <summary>
/// Indicates whether a CoinGecko API call completed successfully.
/// </summary>
public interface IApiResponse
{
    /// <summary>True when the HTTP request and deserialization succeeded.</summary>
    bool HasRequestSucceeded { get; init; }
}
