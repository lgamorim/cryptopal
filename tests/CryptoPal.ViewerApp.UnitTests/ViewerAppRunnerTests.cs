using CryptoPal.Application;
using CryptoPal.Application.CurrentPrice;
using CryptoPal.Application.HistoricalMarketData;
using FluentAssertions;
using NSubstitute;

namespace CryptoPal.ViewerApp.UnitTests;

public class ViewerAppRunnerTests
{
    [Fact]
    public async Task Should_PrintFormattedPricesAndReturnZero_When_PriceCommandIsValid()
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
            .Returns(new CurrentPriceView { CoinPrices = [] });

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
    public async Task Should_PrintHistoricalDataAndReturnZero_When_HistoryCommandIsValid()
    {
        var historicalMarketDataView = new HistoricalMarketDataView
        {
            Coin = "bitcoin",
            Currency = "eur",
            Prices = [new DatedValue("2023-07-04", 28477.64m), new DatedValue("2023-07-05", 28058.67m)],
            MarketCaps = [],
            TotalVolumes = []
        };
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetHistoricalMarketDataAsync(Arg.Any<GetHistoricalMarketDataQuery>(), Arg.Any<CancellationToken>())
            .Returns(historicalMarketDataView);

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
        output.ToString().Should().Contain("history <coin> <currency> <days>");
        await cryptocurrencyService.DidNotReceive().GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>());
        await cryptocurrencyService.DidNotReceive().GetHistoricalMarketDataAsync(Arg.Any<GetHistoricalMarketDataQuery>(), Arg.Any<CancellationToken>());
    }

    public static TheoryData<string[]> InvalidArgs
    {
        get
        {
            var data = new TheoryData<string[]>();
            data.Add([]);
            data.Add(["unknown"]);
            data.Add(["price", "bitcoin"]);
            data.Add(["history", "bitcoin", "eur"]);
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
            .Returns(Task.FromException<CurrentPriceView>(new InvalidOperationException("Service failed")));

        var error = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, new StringWriter(), error);

        var exitCode = await runner.RunAsync(["price", "bitcoin", "eur"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        error.ToString().Trim().Should().Be("Service failed");
    }
}
