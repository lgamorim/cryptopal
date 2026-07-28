using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;

namespace CryptoPal.Core;

/// <summary>
/// Orchestrates cryptocurrency market data queries and maps CoinGecko responses into view models.
/// </summary>
public interface ICryptocurrencyService
{
    /// <summary>Gets the latest price for one or more coins in the requested currencies.</summary>
    Task<CurrentPriceView> GetCurrentPriceAsync(GetCurrentPriceQuery query, CancellationToken cancellationToken = default);

    /// <summary>Gets the latest price for tokens identified by contract address on a platform.</summary>
    Task<TokenPriceView> GetTokenPriceAsync(GetTokenPriceQuery query, CancellationToken cancellationToken = default);

    /// <summary>Gets historical price, market cap, and volume series for a coin.</summary>
    Task<HistoricalMarketDataView> GetHistoricalMarketDataAsync(GetHistoricalMarketDataQuery query, CancellationToken cancellationToken = default);

    /// <summary>Gets detailed metadata and market snapshots for a coin.</summary>
    Task<CoinDataView> GetCoinDataAsync(GetCoinDataQuery query, CancellationToken cancellationToken = default);

    /// <summary>Gets developer repository activity for a coin on a historical date.</summary>
    Task<DeveloperDataView> GetDeveloperDataAsync(GetDeveloperDataQuery query, CancellationToken cancellationToken = default);
}
