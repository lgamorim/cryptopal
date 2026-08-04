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
            return ServiceResult<CurrentPriceView>.Success(new CurrentPriceView { CoinPrices = coinPrices });
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
            return ServiceResult<TokenPriceView>.Success(new TokenPriceView { ContractPrices = contractPrices });
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

            // Map all series before assigning so a mid-mapping failure leaves the view empty rather than partial.
            var mappedPrices = MapToDatedValues(historicalMarketData.Prices);
            var mappedMarketCaps = MapToDatedValues(historicalMarketData.MarketCaps);
            var mappedTotalVolumes = MapToDatedValues(historicalMarketData.TotalVolumes);

            return ServiceResult<HistoricalMarketDataView>.Success(new HistoricalMarketDataView
            {
                Coin = query.Coin,
                Currency = query.Currency,
                Prices = mappedPrices,
                MarketCaps = mappedMarketCaps,
                TotalVolumes = mappedTotalVolumes
            });
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
        return ServiceResult<T>.Failure(MapStatusCode(response.HttpStatusCode), errorMessage);
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

    private static IReadOnlyList<CoinPrice> MapToCoinPrices(IDictionary<string, IDictionary<string, decimal>> cryptoPrices)
    {
        var coinPrices = new List<CoinPrice>(cryptoPrices.Count);
        foreach (var (id, currencyPrices) in cryptoPrices)
        {
            var prices = currencyPrices.Select(pair => new Price(pair.Key, pair.Value)).ToList();
            coinPrices.Add(new CoinPrice { Id = id, Prices = prices });
        }

        return coinPrices;
    }

    private static IReadOnlyList<ContractPrice> MapToContractPrices(IDictionary<string, IDictionary<string, decimal>> tokenPrices)
    {
        var contractPrices = new List<ContractPrice>(tokenPrices.Count);
        foreach (var (address, currencyPrices) in tokenPrices)
        {
            var prices = currencyPrices.Select(pair => new Price(pair.Key, pair.Value)).ToList();
            contractPrices.Add(new ContractPrice { Address = address, Prices = prices });
        }

        return contractPrices;
    }

    private static IList<DatedValue> MapToDatedValues(IEnumerable<MarketDataPoint> points) =>
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

        return new CoinDataView
        {
            Id = coinDetail.Id ?? queryCoin,
            Symbol = coinDetail.Symbol ?? string.Empty,
            Name = coinDetail.Name ?? string.Empty,
            Description = description,
            ImageUrl = imageUrl,
            PriceChangePercentage24h = coinDetail.MarketData?.PriceChangePercentage24h ?? 0,
            MarketSnapshots = MapToMarketSnapshots(coinDetail.MarketData)
        };
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

        return new DeveloperDataView
        {
            Id = coinHistory.Id ?? queryCoin,
            Symbol = coinHistory.Symbol ?? string.Empty,
            Name = coinHistory.Name ?? string.Empty,
            Forks = developerData?.Forks ?? 0,
            Stars = developerData?.Stars ?? 0,
            Subscribers = developerData?.Subscribers ?? 0,
            TotalIssues = developerData?.TotalIssues ?? 0,
            ClosedIssues = developerData?.ClosedIssues ?? 0,
            PullRequestsMerged = developerData?.PullRequestsMerged ?? 0,
            PullRequestContributors = developerData?.PullRequestContributors ?? 0,
            CodeAdditions = codeChanges?.Additions ?? 0,
            CodeDeletions = codeChanges?.Deletions ?? 0,
            CommitCount4Weeks = developerData?.CommitCount4Weeks ?? 0
        };
    }
}
