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

public class CryptocurrencyService(ICoinGeckoClient coinGeckoClient, ILogger<CryptocurrencyService> logger) : ICryptocurrencyService
{
    public async Task<CurrentPriceView> GetCurrentPriceAsync(GetCurrentPriceQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Coins);
        ArgumentNullException.ThrowIfNull(query.Currencies);

        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = query.Coins,
            Currencies = query.Currencies
        };

        IReadOnlyList<CoinPrice> coinPrices = Array.Empty<CoinPrice>();
        try
        {
            var simplePriceResponse = await coinGeckoClient.GetSimplePriceAsync(simplePriceRequest, cancellationToken);
            if (simplePriceResponse.HasRequestSucceeded)
            {
                coinPrices = MapToCoinPrices(simplePriceResponse.CryptocurrencyPrices);
            }
        }
        catch (Exception exception) when (exception is IndexOutOfRangeException or ArgumentOutOfRangeException or InvalidCastException or OverflowException)
        {
            logger.LogError(exception, "Failed to map current price response for coins {Coins}.", string.Join(',', query.Coins));
        }

        return new CurrentPriceView { CoinPrices = coinPrices };
    }

    public async Task<TokenPriceView> GetTokenPriceAsync(GetTokenPriceQuery query, CancellationToken cancellationToken = default)
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

        IReadOnlyList<ContractPrice> contractPrices = Array.Empty<ContractPrice>();
        try
        {
            var simpleTokenPriceResponse = await coinGeckoClient.GetSimpleTokenPriceAsync(simpleTokenPriceRequest, cancellationToken);
            if (simpleTokenPriceResponse.HasRequestSucceeded)
            {
                contractPrices = MapToContractPrices(simpleTokenPriceResponse.TokenPrices);
            }
        }
        catch (Exception exception) when (exception is IndexOutOfRangeException or ArgumentOutOfRangeException or InvalidCastException or OverflowException)
        {
            logger.LogError(exception, "Failed to map token price response for contract addresses {ContractAddresses}.", string.Join(',', query.ContractAddresses));
        }

        return new TokenPriceView { ContractPrices = contractPrices };
    }

    public async Task<HistoricalMarketDataView> GetHistoricalMarketDataAsync(GetHistoricalMarketDataQuery query, CancellationToken cancellationToken = default)
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

        IList<DatedValue> prices = Array.Empty<DatedValue>();
        IList<DatedValue> marketCaps = Array.Empty<DatedValue>();
        IList<DatedValue> totalVolumes = Array.Empty<DatedValue>();
        try
        {
            var coinMarketChartResponse = await coinGeckoClient.GetCoinMarketChartAsync(coinMarketChartRequest, cancellationToken);
            if (coinMarketChartResponse.HasRequestSucceeded)
            {
                var historicalMarketData = coinMarketChartResponse.HistoricalMarketData;

                // Map all series before assigning so a mid-mapping failure leaves the view empty rather than partial.
                var mappedPrices = MapToDatedValues(historicalMarketData.Prices);
                var mappedMarketCaps = MapToDatedValues(historicalMarketData.MarketCaps);
                var mappedTotalVolumes = MapToDatedValues(historicalMarketData.TotalVolumes);

                prices = mappedPrices;
                marketCaps = mappedMarketCaps;
                totalVolumes = mappedTotalVolumes;
            }
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException)
        {
            logger.LogError(exception, "Failed to map historical market data response for coin {Coin}.", query.Coin);
        }

        return new HistoricalMarketDataView
        {
            Coin = query.Coin,
            Currency = query.Currency,
            Prices = prices,
            MarketCaps = marketCaps,
            TotalVolumes = totalVolumes
        };
    }

    public async Task<CoinDataView> GetCoinDataAsync(GetCoinDataQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Coin);

        var coinDataRequest = new CoinDataRequest()
        {
            Coin = query.Coin
        };

        var coinDataView = CreateEmptyCoinDataView(query.Coin);
        try
        {
            var coinDataResponse = await coinGeckoClient.GetCoinDataAsync(coinDataRequest, cancellationToken);
            if (coinDataResponse.HasRequestSucceeded)
            {
                coinDataView = MapToCoinDataView(query.Coin, coinDataResponse.Coin);
            }
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or InvalidCastException or OverflowException)
        {
            logger.LogError(exception, "Failed to map coin data response for coin {Coin}.", query.Coin);
        }

        return coinDataView;
    }

    public async Task<DeveloperDataView> GetDeveloperDataAsync(GetDeveloperDataQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Coin);
        ArgumentNullException.ThrowIfNull(query.Date);

        var coinHistoryRequest = new CoinHistoryRequest()
        {
            Coin = query.Coin,
            Date = query.Date
        };

        var developerDataView = CreateEmptyDeveloperDataView(query.Coin);
        try
        {
            var coinHistoryResponse = await coinGeckoClient.GetCoinHistoryAsync(coinHistoryRequest, cancellationToken);
            if (coinHistoryResponse.HasRequestSucceeded)
            {
                developerDataView = MapToDeveloperDataView(query.Coin, coinHistoryResponse.Coin);
            }
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or InvalidCastException or OverflowException)
        {
            logger.LogError(exception, "Failed to map developer data response for coin {Coin}.", query.Coin);
        }

        return developerDataView;
    }

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
        var currentPrices = marketData?.CurrentPrice;
        if (currentPrices is null)
        {
            return Array.Empty<CoinMarketSnapshot>();
        }

        var snapshots = new List<CoinMarketSnapshot>(currentPrices.Count);
        foreach (var (currency, currentPrice) in currentPrices)
        {
            var marketCap = marketData!.MarketCap is not null && marketData.MarketCap.TryGetValue(currency, out var cap) ? cap : 0;
            var totalVolume = marketData.TotalVolume is not null && marketData.TotalVolume.TryGetValue(currency, out var volume) ? volume : 0;
            snapshots.Add(new CoinMarketSnapshot(currency, currentPrice, marketCap, totalVolume));
        }

        return snapshots;
    }

    private static CoinDataView CreateEmptyCoinDataView(string queryCoin) =>
        new()
        {
            Id = queryCoin,
            Symbol = string.Empty,
            Name = string.Empty,
            Description = string.Empty,
            ImageUrl = string.Empty,
            PriceChangePercentage24h = 0,
            MarketSnapshots = Array.Empty<CoinMarketSnapshot>()
        };

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

    private static DeveloperDataView CreateEmptyDeveloperDataView(string queryCoin) =>
        new()
        {
            Id = queryCoin,
            Symbol = string.Empty,
            Name = string.Empty,
            Forks = 0,
            Stars = 0,
            Subscribers = 0,
            TotalIssues = 0,
            ClosedIssues = 0,
            PullRequestsMerged = 0,
            PullRequestContributors = 0,
            CodeAdditions = 0,
            CodeDeletions = 0,
            CommitCount4Weeks = 0
        };
}