using CryptoPal.Core;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace CryptoPal.ViewerApi.UnitTests;

public class ViewerApiEndpointsTests
{
    [Fact]
    public async Task Should_ReturnCurrentPriceView_When_GetCurrentPriceIsCalled()
    {
        var currentPriceView = new CurrentPriceView(
        [
            new CoinPrice("bitcoin", [new Price("eur", 28135m), new Price("usd", 30628m)])
        ]);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CurrentPriceView>.Success(currentPriceView));

        var result = await ViewerApiEndpoints.GetCurrentPriceAsync(
            cryptocurrencyService,
            ["bitcoin"],
            ["eur", "usd"],
            TestContext.Current.CancellationToken);

        var okResult = result.Should().BeOfType<Ok<CurrentPriceView>>().Subject;
        okResult.Value.Should().BeSameAs(currentPriceView);

        await cryptocurrencyService.Received(1).GetCurrentPriceAsync(
            Arg.Is<GetCurrentPriceQuery>(query =>
                query.Coins.SequenceEqual(new[] { "bitcoin" }) &&
                query.Currencies.SequenceEqual(new[] { "eur", "usd" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnProblemDetailsWith502_When_GetCurrentPriceFailsUpstream()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CurrentPriceView>.Failure(ServiceErrorCode.UpstreamUnavailable, "Failed to retrieve prices from CoinGecko."));

        var result = await ViewerApiEndpoints.GetCurrentPriceAsync(
            cryptocurrencyService,
            ["bitcoin"],
            ["eur"],
            TestContext.Current.CancellationToken);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status502BadGateway);
        problemResult.ProblemDetails.Title.Should().Be(nameof(ServiceErrorCode.UpstreamUnavailable));
        problemResult.ProblemDetails.Detail.Should().Be("Failed to retrieve prices from CoinGecko.");
    }

    [Fact]
    public async Task Should_ReturnTokenPriceView_When_GetTokenPriceIsCalled()
    {
        var tokenPriceView = new TokenPriceView(
        [
            new ContractPrice("0xdac17f958d2ee523a2206206994597c13d831ec7", [new Price("eur", 0.92m), new Price("usd", 1.0m)])
        ]);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetTokenPriceAsync(Arg.Any<GetTokenPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<TokenPriceView>.Success(tokenPriceView));

        var result = await ViewerApiEndpoints.GetTokenPriceAsync(
            cryptocurrencyService,
            "ethereum",
            ["0xdac17f958d2ee523a2206206994597c13d831ec7"],
            ["eur", "usd"],
            TestContext.Current.CancellationToken);

        var okResult = result.Should().BeOfType<Ok<TokenPriceView>>().Subject;
        okResult.Value.Should().BeSameAs(tokenPriceView);

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
        var historicalMarketDataView = new HistoricalMarketDataView(
            "bitcoin",
            "eur",
            [new DatedValue("2023-07-04", 28477.64m)],
            [],
            []);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetHistoricalMarketDataAsync(Arg.Any<GetHistoricalMarketDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<HistoricalMarketDataView>.Success(historicalMarketDataView));

        var result = await ViewerApiEndpoints.GetHistoricalMarketDataAsync(
            cryptocurrencyService,
            "bitcoin",
            "eur",
            7,
            TestContext.Current.CancellationToken);

        var okResult = result.Should().BeOfType<Ok<HistoricalMarketDataView>>().Subject;
        okResult.Value.Should().BeSameAs(historicalMarketDataView);

        await cryptocurrencyService.Received(1).GetHistoricalMarketDataAsync(
            Arg.Is<GetHistoricalMarketDataQuery>(query =>
                query.Coin == "bitcoin" && query.Currency == "eur" && query.Days == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnCoinDataView_When_GetCoinDataIsCalled()
    {
        var coinDataView = new CoinDataView(
            "bitcoin",
            "btc",
            "Bitcoin",
            "Bitcoin is a cryptocurrency.",
            "large.png",
            1.23m,
            [new CoinMarketSnapshot("eur", 28135m, 552996577247m, 13732072142m)]);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCoinDataAsync(Arg.Any<GetCoinDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CoinDataView>.Success(coinDataView));

        var result = await ViewerApiEndpoints.GetCoinDataAsync(
            cryptocurrencyService,
            "bitcoin",
            TestContext.Current.CancellationToken);

        var okResult = result.Should().BeOfType<Ok<CoinDataView>>().Subject;
        okResult.Value.Should().BeSameAs(coinDataView);

        await cryptocurrencyService.Received(1).GetCoinDataAsync(
            Arg.Is<GetCoinDataQuery>(query => query.Coin == "bitcoin"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(ServiceErrorCode.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ServiceErrorCode.RateLimited, StatusCodes.Status429TooManyRequests)]
    public async Task Should_ReturnMappedProblemDetails_When_GetCoinDataFails(ServiceErrorCode errorCode, int expectedStatusCode)
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCoinDataAsync(Arg.Any<GetCoinDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CoinDataView>.Failure(errorCode, "Failed to retrieve coin data from CoinGecko."));

        var result = await ViewerApiEndpoints.GetCoinDataAsync(
            cryptocurrencyService,
            "not-a-real-coin",
            TestContext.Current.CancellationToken);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(expectedStatusCode);
        problemResult.ProblemDetails.Title.Should().Be(errorCode.ToString());
        problemResult.ProblemDetails.Detail.Should().Be("Failed to retrieve coin data from CoinGecko.");
    }

    [Fact]
    public async Task Should_ReturnDeveloperDataView_When_GetDeveloperDataIsCalled()
    {
        var developerDataView = new DeveloperDataView(
            "bitcoin",
            "btc",
            "Bitcoin",
            36262,
            66818,
            3683,
            7338,
            7299,
            11215,
            846,
            1101,
            -1480,
            147);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetDeveloperDataAsync(Arg.Any<GetDeveloperDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<DeveloperDataView>.Success(developerDataView));

        var result = await ViewerApiEndpoints.GetDeveloperDataAsync(
            cryptocurrencyService,
            "bitcoin",
            "30-12-2025",
            TestContext.Current.CancellationToken);

        var okResult = result.Should().BeOfType<Ok<DeveloperDataView>>().Subject;
        okResult.Value.Should().BeSameAs(developerDataView);

        await cryptocurrencyService.Received(1).GetDeveloperDataAsync(
            Arg.Is<GetDeveloperDataQuery>(query => query.Coin == "bitcoin" && query.Date == "30-12-2025"),
            Arg.Any<CancellationToken>());
    }
}
