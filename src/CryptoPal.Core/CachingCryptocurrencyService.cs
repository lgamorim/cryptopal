using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;
using Microsoft.Extensions.Caching.Memory;

namespace CryptoPal.Core;

/// <summary>Caches slow-changing <see cref="ICryptocurrencyService"/> responses.</summary>
internal sealed class CachingCryptocurrencyService(
    CryptocurrencyService inner,
    IMemoryCache memoryCache,
    TimeSpan cacheDuration) : ICryptocurrencyService
{
    public Task<ServiceResult<CurrentPriceView>> GetCurrentPriceAsync(
        GetCurrentPriceQuery query,
        CancellationToken cancellationToken = default) =>
        inner.GetCurrentPriceAsync(query, cancellationToken);

    public Task<ServiceResult<TokenPriceView>> GetTokenPriceAsync(
        GetTokenPriceQuery query,
        CancellationToken cancellationToken = default) =>
        inner.GetTokenPriceAsync(query, cancellationToken);

    public Task<ServiceResult<HistoricalMarketDataView>> GetHistoricalMarketDataAsync(
        GetHistoricalMarketDataQuery query,
        CancellationToken cancellationToken = default) =>
        inner.GetHistoricalMarketDataAsync(query, cancellationToken);

    public async Task<ServiceResult<CoinDataView>> GetCoinDataAsync(
        GetCoinDataQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = CreateCoinDataCacheKey(query.Coin);
        if (memoryCache.TryGetValue(cacheKey, out ServiceResult<CoinDataView>? cachedResult))
        {
            return cachedResult!;
        }

        var result = await inner.GetCoinDataAsync(query, cancellationToken);
        if (result.IsSuccess)
        {
            memoryCache.Set(cacheKey, result, cacheDuration);
        }

        return result;
    }

    public async Task<ServiceResult<DeveloperDataView>> GetDeveloperDataAsync(
        GetDeveloperDataQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = CreateDeveloperDataCacheKey(query.Coin, query.Date);
        if (memoryCache.TryGetValue(cacheKey, out ServiceResult<DeveloperDataView>? cachedResult))
        {
            return cachedResult!;
        }

        var result = await inner.GetDeveloperDataAsync(query, cancellationToken);
        if (result.IsSuccess)
        {
            memoryCache.Set(cacheKey, result, cacheDuration);
        }

        return result;
    }

    private static string CreateCoinDataCacheKey(string coin) => $"coin-data:{coin}";

    private static string CreateDeveloperDataCacheKey(string coin, string date) => $"developer-data:{coin}:{date}";
}
