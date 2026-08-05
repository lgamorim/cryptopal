namespace CryptoPal.ApiClient.CoinGecko;

/// <summary>
/// Indicates whether a CoinGecko API call completed successfully.
/// </summary>
public interface IApiResponse
{
    /// <summary>True when the HTTP request and deserialization succeeded.</summary>
    bool HasRequestSucceeded { get; init; }

    /// <summary>HTTP status code when the upstream request failed; otherwise <c>null</c>.</summary>
    int? HttpStatusCode { get; init; }

    /// <summary>True when the request failed due to an HttpClient timeout.</summary>
    bool IsTimeout { get; init; }
}
