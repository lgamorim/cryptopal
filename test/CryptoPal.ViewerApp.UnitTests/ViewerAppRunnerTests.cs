using CryptoPal.Core;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;
using FluentAssertions;
using NSubstitute;

namespace CryptoPal.ViewerApp.UnitTests;

public class ViewerAppRunnerTests
{
    [Fact]
    public async Task Should_PrintFormattedPricesAndReturnZero_When_PriceCommandIsValid()
    {
        var currentPriceView = new CurrentPriceView(
        [
            new CoinPrice("bitcoin", [new Price("eur", 28135m), new Price("usd", 30628m)])
        ]);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CurrentPriceView>.Success(currentPriceView));

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(["price", "bitcoin", "eur,usd"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        output.ToString().Should().Contain("bitcoin");
        output.ToString().Should().Contain("eur=28135");
        output.ToString().Should().Contain("usd=30628");

        await cryptocurrencyService.Received(1).GetCurrentPriceAsync(
            Arg.Is<GetCurrentPriceQuery>(query =>
                query.Coins.SequenceEqual(new[] { "bitcoin" }) &&
                query.Currencies.SequenceEqual(new[] { "eur", "usd" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_SplitAndTrimCommaSeparatedValues_When_PriceCommandHasSpaces()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CurrentPriceView>.Success(new CurrentPriceView([])));

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        await runner.RunAsync(["price", "bitcoin, ethereum", "eur, usd"], TestContext.Current.CancellationToken);

        await cryptocurrencyService.Received(1).GetCurrentPriceAsync(
            Arg.Is<GetCurrentPriceQuery>(query =>
                query.Coins.SequenceEqual(new[] { "bitcoin", "ethereum" }) &&
                query.Currencies.SequenceEqual(new[] { "eur", "usd" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_PrintFormattedTokenPricesAndReturnZero_When_TokenCommandIsValid()
    {
        var tokenPriceView = new TokenPriceView(
        [
            new ContractPrice("0xdac17f958d2ee523a2206206994597c13d831ec7", [new Price("eur", 0.92m), new Price("usd", 1.0m)])
        ]);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetTokenPriceAsync(Arg.Any<GetTokenPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<TokenPriceView>.Success(tokenPriceView));

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(["token", "ethereum", "0xdac17f958d2ee523a2206206994597c13d831ec7", "eur,usd"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        output.ToString().Should().Contain("0xdac17f958d2ee523a2206206994597c13d831ec7");
        output.ToString().Should().Contain("eur=0.92");
        output.ToString().Should().Contain("usd=1.0");

        await cryptocurrencyService.Received(1).GetTokenPriceAsync(
            Arg.Is<GetTokenPriceQuery>(query =>
                query.AssetPlatformId == "ethereum" &&
                query.ContractAddresses.SequenceEqual(new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7" }) &&
                query.Currencies.SequenceEqual(new[] { "eur", "usd" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_SplitAndTrimCommaSeparatedValues_When_TokenCommandHasSpaces()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetTokenPriceAsync(Arg.Any<GetTokenPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<TokenPriceView>.Success(new TokenPriceView([])));

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        await runner.RunAsync(["token", "ethereum", "0xaaa, 0xbbb", "eur, usd"], TestContext.Current.CancellationToken);

        await cryptocurrencyService.Received(1).GetTokenPriceAsync(
            Arg.Is<GetTokenPriceQuery>(query =>
                query.AssetPlatformId == "ethereum" &&
                query.ContractAddresses.SequenceEqual(new[] { "0xaaa", "0xbbb" }) &&
                query.Currencies.SequenceEqual(new[] { "eur", "usd" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_PrintHistoricalDataAndReturnZero_When_HistoryCommandIsValid()
    {
        var historicalMarketDataView = new HistoricalMarketDataView(
            "bitcoin",
            "eur",
            [new DatedValue("2023-07-04", 28477.64m), new DatedValue("2023-07-05", 28058.67m)],
            [],
            []);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetHistoricalMarketDataAsync(Arg.Any<GetHistoricalMarketDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<HistoricalMarketDataView>.Success(historicalMarketDataView));

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(["history", "bitcoin", "eur", "7"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        output.ToString().Should().Contain("bitcoin/eur");
        output.ToString().Should().Contain("2023-07-04=28477.64");
        output.ToString().Should().Contain("2023-07-05=28058.67");

        await cryptocurrencyService.Received(1).GetHistoricalMarketDataAsync(
            Arg.Is<GetHistoricalMarketDataQuery>(query =>
                query.Coin == "bitcoin" && query.Currency == "eur" && query.Days == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_PrintCoinDataAndReturnZero_When_CoinCommandIsValid()
    {
        var coinDataView = new CoinDataView(
            "bitcoin",
            "btc",
            "Bitcoin",
            "Bitcoin is a cryptocurrency.",
            "large.png",
            1.23m,
            [
                new CoinMarketSnapshot("eur", 28135m, 552996577247m, 13732072142m),
                new CoinMarketSnapshot("usd", 30628m, 601000000000m, 15000000000m)
            ]);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCoinDataAsync(Arg.Any<GetCoinDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CoinDataView>.Success(coinDataView));

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(["coin", "bitcoin"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        output.ToString().Should().Contain("bitcoin (btc) Bitcoin");
        output.ToString().Should().Contain("24h: 1.23%");
        output.ToString().Should().Contain("eur=28135");
        output.ToString().Should().Contain("usd=30628");

        await cryptocurrencyService.Received(1).GetCoinDataAsync(
            Arg.Is<GetCoinDataQuery>(query => query.Coin == "bitcoin"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_PrintDeveloperDataAndReturnZero_When_DeveloperCommandIsValid()
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

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(["developer", "bitcoin", "30-12-2025"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        output.ToString().Should().Contain("bitcoin (btc) Bitcoin");
        output.ToString().Should().Contain("Forks: 36262");
        output.ToString().Should().Contain("Stars: 66818");
        output.ToString().Should().Contain("Pull requests merged: 11215");
        output.ToString().Should().Contain("Code changes (4w): +1101/-1480");
        output.ToString().Should().Contain("Commits (4w): 147");

        await cryptocurrencyService.Received(1).GetDeveloperDataAsync(
            Arg.Is<GetDeveloperDataQuery>(query => query.Coin == "bitcoin" && query.Date == "30-12-2025"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_WriteValidationErrorAndReturnOne_When_PriceCommandHasNoCoins()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        var error = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, new StringWriter(), error);

        var exitCode = await runner.RunAsync(["price", ",", "eur"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        error.ToString().Trim().Should().Be("coins must contain at least one value.");
        await cryptocurrencyService.DidNotReceive().GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_WriteValidationErrorAndReturnOne_When_HistoryDaysAreNotPositive()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        var error = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, new StringWriter(), error);

        var exitCode = await runner.RunAsync(["history", "bitcoin", "eur", "0"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        error.ToString().Trim().Should().Be("days must be greater than zero.");
        await cryptocurrencyService.DidNotReceive().GetHistoricalMarketDataAsync(Arg.Any<GetHistoricalMarketDataQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_WriteValidationErrorAndReturnOne_When_DeveloperDateIsInvalid()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        var error = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, new StringWriter(), error);

        var exitCode = await runner.RunAsync(["developer", "bitcoin", "31-02-2025"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        error.ToString().Trim().Should().Be("date must be a valid calendar date in dd-MM-yyyy format.");
        await cryptocurrencyService.DidNotReceive().GetDeveloperDataAsync(Arg.Any<GetDeveloperDataQuery>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(InvalidArgs))]
    public async Task Should_PrintUsageAndReturnOne_When_ArgsAreInvalid(string[] args)
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(args, TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        output.ToString().Should().Contain("Usage:");
        output.ToString().Should().Contain("price   <coins> <currencies>");
        output.ToString().Should().Contain("token   <platform> <addresses> <currencies>");
        output.ToString().Should().Contain("history <coin> <currency> <days>");
        await cryptocurrencyService.DidNotReceive().GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>());
        await cryptocurrencyService.DidNotReceive().GetTokenPriceAsync(Arg.Any<GetTokenPriceQuery>(), Arg.Any<CancellationToken>());
        await cryptocurrencyService.DidNotReceive().GetHistoricalMarketDataAsync(Arg.Any<GetHistoricalMarketDataQuery>(), Arg.Any<CancellationToken>());
        await cryptocurrencyService.DidNotReceive().GetDeveloperDataAsync(Arg.Any<GetDeveloperDataQuery>(), Arg.Any<CancellationToken>());
    }

    public static TheoryData<string[]> InvalidArgs
    {
        get
        {
            var data = new TheoryData<string[]>();
            data.Add([]);
            data.Add(["unknown"]);
            data.Add(["price", "bitcoin"]);
            data.Add(["token", "ethereum", "0xaaa"]);
            data.Add(["history", "bitcoin", "eur"]);
            data.Add(["coin"]);
            data.Add(["developer", "bitcoin"]);
            return data;
        }
    }

    [Fact]
    public async Task Should_PrintUsageAndReturnOne_When_HistoryDaysIsNotAnInteger()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(["history", "bitcoin", "eur", "abc"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        output.ToString().Should().Contain("Usage:");
        await cryptocurrencyService.DidNotReceive().GetHistoricalMarketDataAsync(Arg.Any<GetHistoricalMarketDataQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_WriteErrorAndReturnOne_When_ServiceThrows()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ServiceResult<CurrentPriceView>>(new InvalidOperationException("Service failed")));

        var error = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, new StringWriter(), error);

        var exitCode = await runner.RunAsync(["price", "bitcoin", "eur"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        error.ToString().Trim().Should().Be("Service failed");
    }

    [Fact]
    public async Task Should_WriteErrorAndReturnOne_When_PriceServiceReturnsFailure()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CurrentPriceView>.Failure(ServiceErrorCode.UpstreamUnavailable, "Failed to retrieve prices from CoinGecko."));

        var output = new StringWriter();
        var error = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, error);

        var exitCode = await runner.RunAsync(["price", "bitcoin", "eur"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        error.ToString().Trim().Should().Be("UpstreamUnavailable: Failed to retrieve prices from CoinGecko.");
        output.ToString().Should().BeEmpty();
    }

    [Fact]
    public async Task Should_WriteErrorAndReturnOne_When_CoinServiceReturnsFailure()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCoinDataAsync(Arg.Any<GetCoinDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CoinDataView>.Failure(ServiceErrorCode.NotFound, "Failed to retrieve coin data from CoinGecko."));

        var output = new StringWriter();
        var error = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, error);

        var exitCode = await runner.RunAsync(["coin", "not-a-real-coin"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        error.ToString().Trim().Should().Be("NotFound: Failed to retrieve coin data from CoinGecko.");
        output.ToString().Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return130_When_CancellationIsRequested()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns<Task<ServiceResult<CurrentPriceView>>>(_ => throw new OperationCanceledException());

        var error = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, new StringWriter(), error);

        var exitCode = await runner.RunAsync(["price", "bitcoin", "eur"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(130);
        error.ToString().Trim().Should().Be("Operation canceled.");
    }

    [Fact]
    public async Task Should_WriteErrorAndReturnOne_When_PriceServiceReturnsRequestTimedOut()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<CurrentPriceView>.Failure(ServiceErrorCode.RequestTimedOut, "Failed to retrieve prices from CoinGecko."));

        var output = new StringWriter();
        var error = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, error);

        var exitCode = await runner.RunAsync(["price", "bitcoin", "eur"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        error.ToString().Trim().Should().Be("RequestTimedOut: Failed to retrieve prices from CoinGecko.");
        output.ToString().Should().BeEmpty();
    }
}
