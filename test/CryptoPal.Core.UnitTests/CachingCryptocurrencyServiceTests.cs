using CryptoPal.ApiClient.CoinGecko;
using CryptoPal.ApiClient.CoinGecko.CoinData;
using CryptoPal.ApiClient.CoinGecko.CoinHistory;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CryptoPal.Core.UnitTests;

public class CachingCryptocurrencyServiceTests
{
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromSeconds(60);

    [Fact]
    public async Task Should_NotCallCoinGeckoClientAgain_When_GetCoinDataCalledTwiceWithinTtl()
    {
        var getCoinDataQuery = new GetCoinDataQuery { Coin = "bitcoin" };
        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinDataAsync(Arg.Any<CoinDataRequest>(), Arg.Any<CancellationToken>())
            .Returns(CreateSuccessfulCoinDataResponse());

        var cachingService = CreateCachingService(coinGeckoClient);

        await cachingService.GetCoinDataAsync(getCoinDataQuery, TestContext.Current.CancellationToken);
        await cachingService.GetCoinDataAsync(getCoinDataQuery, TestContext.Current.CancellationToken);

        await coinGeckoClient.Received(1).GetCoinDataAsync(
            Arg.Is<CoinDataRequest>(request => request.Coin == "bitcoin"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_NotCallCoinGeckoClientAgain_When_GetDeveloperDataCalledTwiceWithinTtl()
    {
        var getDeveloperDataQuery = new GetDeveloperDataQuery { Coin = "bitcoin", Date = "30-12-2025" };
        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinHistoryAsync(Arg.Any<CoinHistoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(CreateSuccessfulDeveloperDataResponse());

        var cachingService = CreateCachingService(coinGeckoClient);

        await cachingService.GetDeveloperDataAsync(getDeveloperDataQuery, TestContext.Current.CancellationToken);
        await cachingService.GetDeveloperDataAsync(getDeveloperDataQuery, TestContext.Current.CancellationToken);

        await coinGeckoClient.Received(1).GetCoinHistoryAsync(
            Arg.Is<CoinHistoryRequest>(request => request.Coin == "bitcoin" && request.Date == "30-12-2025"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_CallCoinGeckoClientAgain_When_GetCurrentPriceCalledTwice()
    {
        var getCurrentPriceQuery = new GetCurrentPriceQuery
        {
            Coins = ["bitcoin"],
            Currencies = ["eur"]
        };
        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetSimplePriceAsync(Arg.Any<SimplePriceRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SimplePriceResponse
            {
                HasRequestSucceeded = true,
                CryptocurrencyPrices = new Dictionary<string, IDictionary<string, decimal>>
                {
                    ["bitcoin"] = new Dictionary<string, decimal> { ["eur"] = 28135m }
                }
            });

        var cachingService = CreateCachingService(coinGeckoClient);

        await cachingService.GetCurrentPriceAsync(getCurrentPriceQuery, TestContext.Current.CancellationToken);
        await cachingService.GetCurrentPriceAsync(getCurrentPriceQuery, TestContext.Current.CancellationToken);

        await coinGeckoClient.Received(2).GetSimplePriceAsync(Arg.Any<SimplePriceRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_CallCoinGeckoClientAgain_When_GetCoinDataFails()
    {
        var getCoinDataQuery = new GetCoinDataQuery { Coin = "bitcoin" };
        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinDataAsync(Arg.Any<CoinDataRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CoinDataResponse
            {
                HasRequestSucceeded = false,
                Coin = new CoinDataResponse.CoinDetail()
            });

        var cachingService = CreateCachingService(coinGeckoClient);

        await cachingService.GetCoinDataAsync(getCoinDataQuery, TestContext.Current.CancellationToken);
        await cachingService.GetCoinDataAsync(getCoinDataQuery, TestContext.Current.CancellationToken);

        await coinGeckoClient.Received(2).GetCoinDataAsync(Arg.Any<CoinDataRequest>(), Arg.Any<CancellationToken>());
    }

    private static CachingCryptocurrencyService CreateCachingService(ICoinGeckoClient coinGeckoClient)
    {
        var inner = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        return new CachingCryptocurrencyService(inner, new MemoryCache(new MemoryCacheOptions()), DefaultCacheDuration);
    }

    private static CoinDataResponse CreateSuccessfulCoinDataResponse() =>
        new()
        {
            HasRequestSucceeded = true,
            Coin = new CoinDataResponse.CoinDetail
            {
                Id = "bitcoin",
                Symbol = "btc",
                Name = "Bitcoin",
                Description = new Dictionary<string, string> { ["en"] = "Bitcoin is a cryptocurrency." },
                Image = new CoinDataResponse.CoinImage { Large = "large.png" },
                MarketData = new CoinDataResponse.CoinMarketData
                {
                    CurrentPrice = new Dictionary<string, decimal> { ["eur"] = 28135m },
                    MarketCap = new Dictionary<string, decimal> { ["eur"] = 552996577247m },
                    TotalVolume = new Dictionary<string, decimal> { ["eur"] = 13732072142m },
                    PriceChangePercentage24h = 1.23m
                }
            }
        };

    private static CoinHistoryResponse CreateSuccessfulDeveloperDataResponse() =>
        new()
        {
            HasRequestSucceeded = true,
            Coin = new CoinHistoryResponse.CoinHistoryDetail
            {
                Id = "bitcoin",
                Symbol = "btc",
                Name = "Bitcoin",
                DeveloperData = new CoinHistoryResponse.CoinDeveloperData
                {
                    Forks = 36262,
                    Stars = 66818,
                    Subscribers = 3683,
                    TotalIssues = 7338,
                    ClosedIssues = 7299,
                    PullRequestsMerged = 11215,
                    PullRequestContributors = 846,
                    CodeAdditionsDeletions4Weeks = new CoinHistoryResponse.CodeChanges { Additions = 1101, Deletions = -1480 },
                    CommitCount4Weeks = 147
                }
            }
        };
}
