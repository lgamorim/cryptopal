using System.Text.Json;
using CryptoPal.Core;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;
using FluentAssertions;
using NSubstitute;

namespace CryptoPal.ViewerApp.UnitTests;

public class ViewerAppRunnerJsonOutputTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Should_OutputJsonAndReturnZero_When_PriceCommandUsesJsonFlag()
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

        var exitCode = await runner.RunAsync(["--json", "price", "bitcoin", "eur,usd"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        JsonSerializer.Deserialize<CurrentPriceView>(output.ToString().Trim(), JsonOptions)
            .Should().BeEquivalentTo(currentPriceView);
        output.ToString().Should().NotContain("eur=28135");
    }

    [Fact]
    public async Task Should_OutputJsonAndReturnZero_When_TokenCommandUsesJsonFlag()
    {
        var tokenPriceView = new TokenPriceView(
        [
            new ContractPrice("0xdac17f958d2ee523a2206206994597c13d831ec7", [new Price("eur", 0.92m)])
        ]);
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        cryptocurrencyService.GetTokenPriceAsync(Arg.Any<GetTokenPriceQuery>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<TokenPriceView>.Success(tokenPriceView));

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(
            ["--json", "token", "ethereum", "0xdac17f958d2ee523a2206206994597c13d831ec7", "eur"],
            TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        JsonSerializer.Deserialize<TokenPriceView>(output.ToString().Trim(), JsonOptions)
            .Should().BeEquivalentTo(tokenPriceView);
    }

    [Fact]
    public async Task Should_OutputJsonAndReturnZero_When_HistoryCommandUsesJsonFlag()
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

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(["--json", "history", "bitcoin", "eur", "7"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        JsonSerializer.Deserialize<HistoricalMarketDataView>(output.ToString().Trim(), JsonOptions)
            .Should().BeEquivalentTo(historicalMarketDataView);
        output.ToString().Should().NotContain("bitcoin/eur");
    }

    [Fact]
    public async Task Should_OutputJsonAndReturnZero_When_CoinCommandUsesJsonFlag()
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

        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(["--json", "coin", "bitcoin"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        JsonSerializer.Deserialize<CoinDataView>(output.ToString().Trim(), JsonOptions)
            .Should().BeEquivalentTo(coinDataView);
        output.ToString().Should().NotContain("24h:");
    }

    [Fact]
    public async Task Should_OutputJsonAndReturnZero_When_DeveloperCommandUsesJsonFlag()
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

        var exitCode = await runner.RunAsync(["--json", "developer", "bitcoin", "30-12-2025"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(0);
        JsonSerializer.Deserialize<DeveloperDataView>(output.ToString().Trim(), JsonOptions)
            .Should().BeEquivalentTo(developerDataView);
        output.ToString().Should().NotContain("Forks:");
    }

    [Fact]
    public async Task Should_PrintUsageAndReturnOne_When_JsonFlagHasNoCommand()
    {
        var cryptocurrencyService = Substitute.For<ICryptocurrencyService>();
        var output = new StringWriter();
        var runner = new ViewerAppRunner(cryptocurrencyService, output, new StringWriter());

        var exitCode = await runner.RunAsync(["--json"], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        output.ToString().Should().Contain("[--json]");
        await cryptocurrencyService.DidNotReceive().GetCurrentPriceAsync(Arg.Any<GetCurrentPriceQuery>(), Arg.Any<CancellationToken>());
    }
}
