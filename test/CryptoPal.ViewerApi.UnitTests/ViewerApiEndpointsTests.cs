using CryptoPal.Core;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;
using FluentAssertions;
using NSubstitute;

namespace CryptoPal.ViewerApi.UnitTests;

public class ViewerApiEndpointsTests
{
    [Fact]
    public async Task Should_ReturnCurrentPriceView_When_GetCurrentPriceIsCalled()
    {
        var currentPriceView = new CurrentPriceView
        {
            CoinPrices =
            [
                new CoinPrice
                {
                    Id = "bitcoin",
                    Prices = [new Price("eur", 28135m), new Price("usd", 30628m)]
                }
            ]
        };
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(currentPriceView);

        var result = await ViewerApiEndpoints.GetCurrentPriceAsync(
            cryptocurrencyService,
            ["bitcoin"],
            ["eur", "usd"],
            TestContext.Current.CancellationToken);

        result.Value.Should().BeSameAs(currentPriceView);

        await cryptocurrencyService.Received(1).GetCurrentPriceAsync(
            Arg.Is<GetCurrentPriceQuery>(query =>
                query.Coins.SequenceEqual(new[] { "bitcoin" }) &&
                query.Currencies.SequenceEqual(new[] { "eur", "usd" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnTokenPriceView_When_GetTokenPriceIsCalled()
    {
        var tokenPriceView = new TokenPriceView
        {
            ContractPrices =
            [
                new ContractPrice
                {
                    Address = "0xdac17f958d2ee523a2206206994597c13d831ec7",
                    Prices = [new Price("eur", 0.92m), new Price("usd", 1.0m)]
                }
            ]
        };
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetTokenPriceAsync(Arg.Any<GetTokenPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(tokenPriceView);

        var result = await ViewerApiEndpoints.GetTokenPriceAsync(
            cryptocurrencyService,
            "ethereum",
            ["0xdac17f958d2ee523a2206206994597c13d831ec7"],
            ["eur", "usd"],
            TestContext.Current.CancellationToken);

        result.Value.Should().BeSameAs(tokenPriceView);

        await cryptocurrencyService.Received(1).GetTokenPriceAsync(
            Arg.Is<GetTokenPriceQuery>(query =>
                query.AssetPlatformId == "ethereum" &&
                query.ContractAddresses.SequenceEqual(new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7" }) &&
                query.Currencies.SequenceEqual(new[] { "eur", "usd" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnHistoricalMarketDataView_When_GetHistoricalMarketDataIsCalled()
    {
        var historicalMarketDataView = new HistoricalMarketDataView
        {
            Coin = "bitcoin",
            Currency = "eur",
            Prices = [new DatedValue("2023-07-04", 28477.64m)],
            MarketCaps = [],
            TotalVolumes = []
        };
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetHistoricalMarketDataAsync(Arg.Any<GetHistoricalMarketDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(historicalMarketDataView);

        var result = await ViewerApiEndpoints.GetHistoricalMarketDataAsync(
            cryptocurrencyService,
            "bitcoin",
            "eur",
            7,
            TestContext.Current.CancellationToken);

        result.Value.Should().BeSameAs(historicalMarketDataView);

        await cryptocurrencyService.Received(1).GetHistoricalMarketDataAsync(
            Arg.Is<GetHistoricalMarketDataQuery>(query =>
                query.Coin == "bitcoin" && query.Currency == "eur" && query.Days == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnCoinDataView_When_GetCoinDataIsCalled()
    {
        var coinDataView = new CoinDataView
        {
            Id = "bitcoin",
            Symbol = "btc",
            Name = "Bitcoin",
            Description = "Bitcoin is a cryptocurrency.",
            ImageUrl = "large.png",
            PriceChangePercentage24h = 1.23m,
            MarketSnapshots = [new CoinMarketSnapshot("eur", 28135m, 552996577247m, 13732072142m)]
        };
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCoinDataAsync(Arg.Any<GetCoinDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(coinDataView);

        var result = await ViewerApiEndpoints.GetCoinDataAsync(
            cryptocurrencyService,
            "bitcoin",
            TestContext.Current.CancellationToken);

        result.Value.Should().BeSameAs(coinDataView);

        await cryptocurrencyService.Received(1).GetCoinDataAsync(
            Arg.Is<GetCoinDataQuery>(query => query.Coin == "bitcoin"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnDeveloperDataView_When_GetDeveloperDataIsCalled()
    {
        var developerDataView = new DeveloperDataView
        {
            Id = "bitcoin",
            Symbol = "btc",
            Name = "Bitcoin",
            Forks = 36262,
            Stars = 66818,
            Subscribers = 3683,
            TotalIssues = 7338,
            ClosedIssues = 7299,
            PullRequestsMerged = 11215,
            PullRequestContributors = 846,
            CodeAdditions = 1101,
            CodeDeletions = -1480,
            CommitCount4Weeks = 147
        };
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetDeveloperDataAsync(Arg.Any<GetDeveloperDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(developerDataView);

        var result = await ViewerApiEndpoints.GetDeveloperDataAsync(
            cryptocurrencyService,
            "bitcoin",
            "30-12-2025",
            TestContext.Current.CancellationToken);

        result.Value.Should().BeSameAs(developerDataView);

        await cryptocurrencyService.Received(1).GetDeveloperDataAsync(
            Arg.Is<GetDeveloperDataQuery>(query => query.Coin == "bitcoin" && query.Date == "30-12-2025"),
            Arg.Any<CancellationToken>());
    }
}
