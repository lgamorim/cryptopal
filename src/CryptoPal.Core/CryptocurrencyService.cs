using System.Globalization;
using CryptoPal.ApiClient.CoinGecko;
using CryptoPal.ApiClient.CoinGecko.CoinData;
using CryptoPal.ApiClient.CoinGecko.CoinHistory;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.ApiClient.CoinGecko.SimpleTokenPrice;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;
using Microsoft.Extensions.Logging;

namespace CryptoPal.Core;

/// <summary>Default implementation of <see cref="ICryptocurrencyService"/>.</summary>
public class CryptocurrencyService(ICoinGeckoClient coinGeckoClient, ILogger<CryptocurrencyService> logger) : ICryptocurrencyService
{
    /// <inheritdoc />
    public async Task<ServiceResult<CurrentPriceView>> GetCurrentPriceAsync(GetCurrentPriceQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Coins);
        ArgumentNullException.ThrowIfNull(query.Currencies);

        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = query.Coins,
            Currencies = query.Currencies
        };

        try
        {
            var simplePriceResponse = await coinGeckoClient.GetSimplePriceAsync(simplePriceRequest, cancellationToken);
            if (!simplePriceResponse.HasRequestSucceeded)
            {
                return CreateUpstreamFailure<CurrentPriceView>(simplePriceResponse, "Failed to retrieve prices from CoinGecko.");
            }

            var coinPrices = MapToCoinPrices(simplePriceResponse.CryptocurrencyPrices);
            var missingCoins = FindMissingIdentifiers(query.Coins, coinPrices.Select(price => price.Id));
            if (missingCoins.Count > 0)
            {
                return ServiceResult<CurrentPriceView>.Failure(
                    ServiceErrorCode.NotFound,
                    $"One or more coins were not found: {string.Join(", ", missingCoins)}.");
            }

            return ServiceResult<CurrentPriceView>.Success(new CurrentPriceView(coinPrices));
        }
        catch (Exception exception) when (exception is IndexOutOfRangeException or ArgumentOutOfRangeException or InvalidCastException or OverflowException)
        {
            return CreateMappingFailure<CurrentPriceView>(exception, "Failed to map current price response for coins {Coins}.", string.Join(',', query.Coins));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TokenPriceView>> GetTokenPriceAsync(GetTokenPriceQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.AssetPlatformId);
        ArgumentNullException.ThrowIfNull(query.ContractAddresses);
        ArgumentNullException.ThrowIfNull(query.Currencies);

        var simpleTokenPriceRequest = new SimpleTokenPriceRequest()
        {
            AssetPlatformId = query.AssetPlatformId,
            ContractAddresses = query.ContractAddresses,
            Currencies = query.Currencies
        };

        try
        {
            var simpleTokenPriceResponse = await coinGeckoClient.GetSimpleTokenPriceAsync(simpleTokenPriceRequest, cancellationToken);
            if (!simpleTokenPriceResponse.HasRequestSucceeded)
            {
                return CreateUpstreamFailure<TokenPriceView>(simpleTokenPriceResponse, "Failed to retrieve token prices from CoinGecko.");
            }

            var contractPrices = MapToContractPrices(simpleTokenPriceResponse.TokenPrices);
            var missingContracts = FindMissingIdentifiers(
                query.ContractAddresses,
                contractPrices.Select(price => price.Address),
                StringComparer.OrdinalIgnoreCase);
            if (missingContracts.Count > 0)
            {
                return ServiceResult<TokenPriceView>.Failure(
                    ServiceErrorCode.NotFound,
                    $"One or more contract addresses were not found: {string.Join(", ", missingContracts)}.");
            }

            return ServiceResult<TokenPriceView>.Success(new TokenPriceView(contractPrices));
        }
        catch (Exception exception) when (exception is IndexOutOfRangeException or ArgumentOutOfRangeException or InvalidCastException or OverflowException)
        {
            return CreateMappingFailure<TokenPriceView>(exception, "Failed to map token price response for contract addresses {ContractAddresses}.", string.Join(',', query.ContractAddresses));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<HistoricalMarketDataView>> GetHistoricalMarketDataAsync(GetHistoricalMarketDataQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Coin);
        ArgumentNullException.ThrowIfNull(query.Currency);

        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = query.Coin,
            Currency = query.Currency,
            Days = query.Days
        };

        try
        {
            var coinMarketChartResponse = await coinGeckoClient.GetCoinMarketChartAsync(coinMarketChartRequest, cancellationToken);
            if (!coinMarketChartResponse.HasRequestSucceeded)
            {
                return CreateUpstreamFailure<HistoricalMarketDataView>(coinMarketChartResponse, "Failed to retrieve historical market data from CoinGecko.");
            }

            var historicalMarketData = coinMarketChartResponse.HistoricalMarketData;
            if (IsEmptyHistoricalMarketData(historicalMarketData))
            {
                return ServiceResult<HistoricalMarketDataView>.Failure(
                    ServiceErrorCode.NotFound,
                    $"Historical market data was not found for coin '{query.Coin}'.");
            }

            // Map all series before assigning so a mid-mapping failure leaves the view empty rather than partial.
            var mappedPrices = MapToDatedValues(historicalMarketData.Prices);
            var mappedMarketCaps = MapToDatedValues(historicalMarketData.MarketCaps);
            var mappedTotalVolumes = MapToDatedValues(historicalMarketData.TotalVolumes);

            return ServiceResult<HistoricalMarketDataView>.Success(new HistoricalMarketDataView(
                query.Coin,
                query.Currency,
                mappedPrices,
                mappedMarketCaps,
                mappedTotalVolumes));
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException)
        {
            return CreateMappingFailure<HistoricalMarketDataView>(exception, "Failed to map historical market data response for coin {Coin}.", query.Coin);
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CoinDataView>> GetCoinDataAsync(GetCoinDataQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Coin);

        var coinDataRequest = new CoinDataRequest()
        {
            Coin = query.Coin
        };

        try
        {
            var coinDataResponse = await coinGeckoClient.GetCoinDataAsync(coinDataRequest, cancellationToken);
            if (!coinDataResponse.HasRequestSucceeded)
            {
                return CreateUpstreamFailure<CoinDataView>(coinDataResponse, "Failed to retrieve coin data from CoinGecko.");
            }

            if (string.IsNullOrWhiteSpace(coinDataResponse.Coin.Id))
            {
                return ServiceResult<CoinDataView>.Failure(
                    ServiceErrorCode.NotFound,
                    $"Coin data was not found for coin '{query.Coin}'.");
            }

            var coinDataView = MapToCoinDataView(query.Coin, coinDataResponse.Coin);
            return ServiceResult<CoinDataView>.Success(coinDataView);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or InvalidCastException or OverflowException)
        {
            return CreateMappingFailure<CoinDataView>(exception, "Failed to map coin data response for coin {Coin}.", query.Coin);
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<DeveloperDataView>> GetDeveloperDataAsync(GetDeveloperDataQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Coin);
        ArgumentNullException.ThrowIfNull(query.Date);

        var coinHistoryRequest = new CoinHistoryRequest()
        {
            Coin = query.Coin,
            Date = query.Date
        };

        try
        {
            var coinHistoryResponse = await coinGeckoClient.GetCoinHistoryAsync(coinHistoryRequest, cancellationToken);
            if (!coinHistoryResponse.HasRequestSucceeded)
            {
                return CreateUpstreamFailure<DeveloperDataView>(coinHistoryResponse, "Failed to retrieve developer data from CoinGecko.");
            }

            if (string.IsNullOrWhiteSpace(coinHistoryResponse.Coin.Id))
            {
                return ServiceResult<DeveloperDataView>.Failure(
                    ServiceErrorCode.NotFound,
                    $"Developer data was not found for coin '{query.Coin}'.");
            }

            var developerDataView = MapToDeveloperDataView(query.Coin, coinHistoryResponse.Coin);
            return ServiceResult<DeveloperDataView>.Success(developerDataView);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or InvalidCastException or OverflowException)
        {
            return CreateMappingFailure<DeveloperDataView>(exception, "Failed to map developer data response for coin {Coin}.", query.Coin);
        }
    }

    private ServiceResult<T> CreateUpstreamFailure<T>(IApiResponse response, string errorMessage)
    {
        logger.LogWarning("CoinGecko request failed with status {StatusCode}.", response.HttpStatusCode);
        var errorCode = response.IsTimeout
            ? ServiceErrorCode.RequestTimedOut
            : MapStatusCode(response.HttpStatusCode);
        return ServiceResult<T>.Failure(errorCode, errorMessage);
    }

    private ServiceResult<T> CreateMappingFailure<T>(Exception exception, string messageTemplate, params object?[] args)
    {
        logger.LogError(exception, messageTemplate, args);
        return ServiceResult<T>.Failure(ServiceErrorCode.ResponseMappingFailed, "Failed to process the upstream response.");
    }

    private static ServiceErrorCode MapStatusCode(int? httpStatusCode) => httpStatusCode switch
    {
        404 => ServiceErrorCode.NotFound,
        429 => ServiceErrorCode.RateLimited,
        _ => ServiceErrorCode.UpstreamUnavailable
    };

    private static List<string> FindMissingIdentifiers(
        IEnumerable<string> requested,
        IEnumerable<string> returned,
        StringComparer? comparer = null)
    {
        comparer ??= StringComparer.OrdinalIgnoreCase;
        var returnedIds = returned.ToHashSet(comparer);
        return requested
            .Select(id => id.Trim())
            .Where(id => !string.IsNullOrEmpty(id))
            .Where(id => !returnedIds.Contains(id))
            .ToList();
    }

    private static bool IsEmptyHistoricalMarketData(CoinMarketChartResponse.MarketChart marketChart) =>
        marketChart.Prices.Count == 0
        && marketChart.MarketCaps.Count == 0
        && marketChart.TotalVolumes.Count == 0;

    private static IReadOnlyList<CoinPrice> MapToCoinPrices(IDictionary<string, IDictionary<string, decimal>> cryptoPrices)
    {
        var coinPrices = new List<CoinPrice>(cryptoPrices.Count);
        foreach (var (id, currencyPrices) in cryptoPrices)
        {
            var prices = currencyPrices.Select(pair => new Price(pair.Key, pair.Value)).ToList();
            coinPrices.Add(new CoinPrice(id, prices));
        }

        return coinPrices;
    }

    private static IReadOnlyList<ContractPrice> MapToContractPrices(IDictionary<string, IDictionary<string, decimal>> tokenPrices)
    {
        var contractPrices = new List<ContractPrice>(tokenPrices.Count);
        foreach (var (address, currencyPrices) in tokenPrices)
        {
            var prices = currencyPrices.Select(pair => new Price(pair.Key, pair.Value)).ToList();
            contractPrices.Add(new ContractPrice(address, prices));
        }

        return contractPrices;
    }

    private static IReadOnlyList<DatedValue> MapToDatedValues(IEnumerable<MarketDataPoint> points) =>
        points
            .Select(point => new DatedValue(
                DateTimeOffset.FromUnixTimeMilliseconds(point.TimestampMs).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                point.Value))
            .ToList();

    private static CoinDataView MapToCoinDataView(string queryCoin, CoinDataResponse.CoinDetail coinDetail)
    {
        var description = coinDetail.Description is not null && coinDetail.Description.TryGetValue("en", out var english)
            ? english
            : string.Empty;

        var imageUrl = coinDetail.Image?.Large
            ?? coinDetail.Image?.Small
            ?? coinDetail.Image?.Thumb
            ?? string.Empty;

        return new CoinDataView(
            coinDetail.Id ?? queryCoin,
            coinDetail.Symbol ?? string.Empty,
            coinDetail.Name ?? string.Empty,
            description,
            imageUrl,
            coinDetail.MarketData?.PriceChangePercentage24h ?? 0,
            MapToMarketSnapshots(coinDetail.MarketData));
    }

    private static IReadOnlyList<CoinMarketSnapshot> MapToMarketSnapshots(CoinDataResponse.CoinMarketData? marketData)
    {
        if (marketData?.CurrentPrice is not { } currentPrices)
        {
            return Array.Empty<CoinMarketSnapshot>();
        }

        var snapshots = new List<CoinMarketSnapshot>(currentPrices.Count);
        foreach (var (currency, currentPrice) in currentPrices)
        {
            var marketCap = marketData.MarketCap is not null && marketData.MarketCap.TryGetValue(currency, out var cap) ? cap : 0;
            var totalVolume = marketData.TotalVolume is not null && marketData.TotalVolume.TryGetValue(currency, out var volume) ? volume : 0;
            snapshots.Add(new CoinMarketSnapshot(currency, currentPrice, marketCap, totalVolume));
        }

        return snapshots;
    }

    private static DeveloperDataView MapToDeveloperDataView(string queryCoin, CoinHistoryResponse.CoinHistoryDetail coinHistory)
    {
        var developerData = coinHistory.DeveloperData;
        var codeChanges = developerData?.CodeAdditionsDeletions4Weeks;

        return new DeveloperDataView(
            coinHistory.Id ?? queryCoin,
            coinHistory.Symbol ?? string.Empty,
            coinHistory.Name ?? string.Empty,
            developerData?.Forks ?? 0,
            developerData?.Stars ?? 0,
            developerData?.Subscribers ?? 0,
            developerData?.TotalIssues ?? 0,
            developerData?.ClosedIssues ?? 0,
            developerData?.PullRequestsMerged ?? 0,
            developerData?.PullRequestContributors ?? 0,
            codeChanges?.Additions ?? 0,
            codeChanges?.Deletions ?? 0,
            developerData?.CommitCount4Weeks ?? 0);
    }
}
