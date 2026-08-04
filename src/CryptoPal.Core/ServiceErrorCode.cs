namespace CryptoPal.Core;

/// <summary>Stable error codes surfaced by <see cref="ICryptocurrencyService"/>.</summary>
public enum ServiceErrorCode
{
    /// <summary>The requested resource was not found upstream.</summary>
    NotFound,

    /// <summary>The upstream API rate limit was exceeded.</summary>
    RateLimited,

    /// <summary>The upstream API was unavailable or returned an unexpected response.</summary>
    UpstreamUnavailable,

    /// <summary>The upstream response could not be mapped into a view model.</summary>
    ResponseMappingFailed
}
