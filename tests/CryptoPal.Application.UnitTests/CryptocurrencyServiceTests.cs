using CryptoPal.ApiClient.CoinGecko;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.Application.CurrentPrice;
using CryptoPal.Application.HistoricalMarketData;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CryptoPal.Application.UnitTests;

public class CryptocurrencyServiceTests
{
    [Fact]
    public async Task Should_CallGetSimplePriceAndReturnViewWithCorrectMapping_When_GetCurrentPriceQueryIsValid()
    {
        var getCurrentPriceQuery = new GetCurrentPriceQuery()
        {
            Coins = new[] { "bitcoin", "ethereum", "cardano" },
            Currencies = new[] { "eur", "usd", "gbp", "jpy" }
        };
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = getCurrentPriceQuery.Coins,
            Currencies = getCurrentPriceQuery.Currencies
        };
        var simplePriceResponse = new SimplePriceResponse()
        {
            HasRequestSucceeded = true,
            CryptocurrencyPrices = new Dictionary<string, IDictionary<string, decimal>>()
            {
                { "bitcoin", new Dictionary<string, decimal> { { "eur", 28135 }, { "usd", 30628 }, { "gbp", 24166 }, { "jpy", 4429566 } } },
                { "ethereum", new Dictionary<string, decimal> { { "eur", 1799 }, { "usd", 1958 }, { "gbp", 1545.29m }, { "jpy", 283242 } } },
                { "cardano", new Dictionary<string, decimal> { { "eur", 0.269991m }, { "usd", 0.293915m }, { "gbp", 0.231909m }, { "jpy", 42.51m } } }
            }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetSimplePriceAsync(Arg.Any<SimplePriceRequest>(), Arg.Any<CancellationToken>())
            .Returns(simplePriceResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var currentPriceView = await cryptocurrencyService.GetCurrentPriceAsync(getCurrentPriceQuery, TestContext.Current.CancellationToken);

        Func<SimplePriceRequest, SimplePriceRequest, bool> isExpectedRequest = (request, expected) =>
            Equals(request.Coins, expected.Coins) && Equals(request.Currencies, expected.Currencies);
        await coinGeckoClient.Received(1).GetSimplePriceAsync(Arg.Is<SimplePriceRequest>(request => isExpectedRequest(request, simplePriceRequest)), Arg.Any<CancellationToken>());

        currentPriceView.Should().NotBeNull();
        currentPriceView.CoinPrices.Should().NotBeNull();
        currentPriceView.CoinPrices.Count().Should().Be(3);
        AssertCurrentPriceViewCoinPrices(currentPriceView.CoinPrices);

        void AssertCurrentPriceViewCoinPrices(IEnumerable<CoinPrice> coinPrices)
        {
            var sampledCryptoPrices = simplePriceResponse.CryptocurrencyPrices;
            foreach (var coinPrice in coinPrices)
            {
                sampledCryptoPrices.Should().ContainKey(coinPrice.Id);
                var sampledCoinPrice = sampledCryptoPrices[coinPrice.Id];
                foreach (var price in coinPrice.Prices)
                {
                    sampledCoinPrice.Should().ContainKey(price.Currency);
                    price.Value.Should().Be(sampledCoinPrice[price.Currency]);
                }
            }
        }
    }

    [Fact]
    public async Task Should_GetCurrentPriceThrowArgumentNullException_When_GetCurrentPriceQueryIsNull()
    {
        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetCurrentPriceAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'query')");
    }

    [Fact]
    public async Task Should_GetCurrentPriceThrowArgumentNullException_When_GetCurrentPriceQueryCurrenciesIsNull()
    {
        var getCurrentPriceQuery = new GetCurrentPriceQuery()
        {
            Coins = new[] { "bitcoin", "ethereum", "cardano" },
            Currencies = null!
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetCurrentPriceAsync(getCurrentPriceQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.Currencies");
    }

    [Fact]
    public async Task Should_GetCurrentPriceThrowArgumentNullException_When_GetCurrentPriceQueryCoinsIsNull()
    {
        var getCurrentPriceQuery = new GetCurrentPriceQuery()
        {
            Coins = null!,
            Currencies = new[] { "eur", "usd", "gbp", "jpy" }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetCurrentPriceAsync(getCurrentPriceQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.Coins");
    }

    [Fact]
    public async Task Should_CallGetHistoricalMarketDataAndReturnViewWithCorrectMapping_When_GetHistoricalMarketDataQueryIsValid()
    {
        var getHistoricalMarketDataQuery = new GetHistoricalMarketDataQuery()
        {
            Coin = "bitcoin",
            Currency = "eur",
            Days = 1
        };
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = getHistoricalMarketDataQuery.Coin,
            Currency = getHistoricalMarketDataQuery.Currency,
            Days = getHistoricalMarketDataQuery.Days
        };
        var coinMarketChartResponse = new CoinMarketChartResponse()
        {
            HasRequestSucceeded = true,
            HistoricalMarketData = new CoinMarketChartResponse.MarketChart()
            {
                Prices = new List<MarketDataPoint> { new(1688468488622, 28477.63754439077m), new(1688554643000, 28058.665368361602m) },
                MarketCaps = new List<MarketDataPoint> { new(1688468488622, 552996577247.077m), new(1688554643000, 544504299176.5765m) },
                TotalVolumes = new List<MarketDataPoint> { new(1688468488622, 13732072142.597347m), new(1688554643000, 9014016349.460764m) }
            }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinMarketChartAsync(Arg.Any<CoinMarketChartRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinMarketChartResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var historicalMarketDataView = await cryptocurrencyService.GetHistoricalMarketDataAsync(getHistoricalMarketDataQuery, TestContext.Current.CancellationToken);

        Func<CoinMarketChartRequest, CoinMarketChartRequest, bool> isExpectedRequest = (request, expected) =>
            request.Coin == expected.Coin && request.Currency == expected.Currency && request.Days == expected.Days;
        await coinGeckoClient.Received(1).GetCoinMarketChartAsync(Arg.Is<CoinMarketChartRequest>(request => isExpectedRequest(request, coinMarketChartRequest)), Arg.Any<CancellationToken>());

        historicalMarketDataView.Should().NotBeNull();
        historicalMarketDataView.Coin.Should().Be(coinMarketChartRequest.Coin);
        historicalMarketDataView.Currency.Should().Be(coinMarketChartRequest.Currency);
        historicalMarketDataView.Prices.Should().NotBeNull().And.HaveCount(2);
        historicalMarketDataView.Prices.Should().BeEquivalentTo(new List<DatedValue> { new("2023-07-04", 28477.63754439077m), new("2023-07-05", 28058.665368361602m) });
        historicalMarketDataView.MarketCaps.Should().NotBeNull().And.HaveCount(2);
        historicalMarketDataView.MarketCaps.Should().BeEquivalentTo(new List<DatedValue> { new("2023-07-04", 552996577247.077m), new("2023-07-05", 544504299176.5765m) });
        historicalMarketDataView.TotalVolumes.Should().NotBeNull().And.HaveCount(2);
        historicalMarketDataView.TotalVolumes.Should().BeEquivalentTo(new List<DatedValue> { new("2023-07-04", 13732072142.597347m), new("2023-07-05", 9014016349.460764m) });
    }

    [Fact]
    public async Task Should_CallGetHistoricalMarketDataThrowArgumentNullException_When_GetHistoricalMarketDataQueryIsNull()
    {
        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetHistoricalMarketDataAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'query')");
    }

    [Fact]
    public async Task Should_CallGetHistoricalMarketDataThrowArgumentNullException_When_GetHistoricalMarketDataQueryCurrencyIsNull()
    {
        var getHistoricalMarketDataQuery = new GetHistoricalMarketDataQuery()
        {
            Coin = "bitcoin",
            Currency = null!,
            Days = 1
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetHistoricalMarketDataAsync(getHistoricalMarketDataQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.Currency");
    }

    [Fact]
    public async Task Should_CallGetHistoricalMarketDataThrowArgumentNullException_When_GetHistoricalMarketDataQueryCoinIsNull()
    {
        var getHistoricalMarketDataQuery = new GetHistoricalMarketDataQuery()
        {
            Coin = null!,
            Currency = "eur",
            Days = 1
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetHistoricalMarketDataAsync(getHistoricalMarketDataQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.Coin");
    }

    [Fact]
    public async Task Should_GetCurrentPriceReturnEmptyView_When_SimplePriceResponseIsUnsuccessful()
    {
        var getCurrentPriceQuery = new GetCurrentPriceQuery()
        {
            Coins = new[] { "bitcoin" },
            Currencies = new[] { "eur" }
        };
        var simplePriceResponse = new SimplePriceResponse()
        {
            HasRequestSucceeded = false,
            CryptocurrencyPrices = new Dictionary<string, IDictionary<string, decimal>>()
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetSimplePriceAsync(Arg.Any<SimplePriceRequest>(), Arg.Any<CancellationToken>())
            .Returns(simplePriceResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var currentPriceView = await cryptocurrencyService.GetCurrentPriceAsync(getCurrentPriceQuery, TestContext.Current.CancellationToken);

        currentPriceView.Should().NotBeNull();
        currentPriceView.CoinPrices.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Should_GetHistoricalMarketDataReturnEmptyView_When_CoinMarketChartResponseIsUnsuccessful()
    {
        var getHistoricalMarketDataQuery = new GetHistoricalMarketDataQuery()
        {
            Coin = "bitcoin",
            Currency = "eur",
            Days = 1
        };
        var coinMarketChartResponse = new CoinMarketChartResponse()
        {
            HasRequestSucceeded = false,
            HistoricalMarketData = new CoinMarketChartResponse.MarketChart()
            {
                Prices = new List<MarketDataPoint>(),
                MarketCaps = new List<MarketDataPoint>(),
                TotalVolumes = new List<MarketDataPoint>()
            }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinMarketChartAsync(Arg.Any<CoinMarketChartRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinMarketChartResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var historicalMarketDataView = await cryptocurrencyService.GetHistoricalMarketDataAsync(getHistoricalMarketDataQuery, TestContext.Current.CancellationToken);

        historicalMarketDataView.Should().NotBeNull();
        historicalMarketDataView.Coin.Should().Be(getHistoricalMarketDataQuery.Coin);
        historicalMarketDataView.Currency.Should().Be(getHistoricalMarketDataQuery.Currency);
        historicalMarketDataView.Prices.Should().NotBeNull().And.BeEmpty();
        historicalMarketDataView.MarketCaps.Should().NotBeNull().And.BeEmpty();
        historicalMarketDataView.TotalVolumes.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Should_GetHistoricalMarketDataReturnEmptyView_When_ResponseTimestampIsOutOfRange()
    {
        var getHistoricalMarketDataQuery = new GetHistoricalMarketDataQuery()
        {
            Coin = "bitcoin",
            Currency = "eur",
            Days = 1
        };
        var coinMarketChartResponse = new CoinMarketChartResponse()
        {
            HasRequestSucceeded = true,
            HistoricalMarketData = new CoinMarketChartResponse.MarketChart()
            {
                Prices = new List<MarketDataPoint> { new(long.MaxValue, 28477.63754439077m) },
                MarketCaps = new List<MarketDataPoint>(),
                TotalVolumes = new List<MarketDataPoint>()
            }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinMarketChartAsync(Arg.Any<CoinMarketChartRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinMarketChartResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var historicalMarketDataView = await cryptocurrencyService.GetHistoricalMarketDataAsync(getHistoricalMarketDataQuery, TestContext.Current.CancellationToken);

        historicalMarketDataView.Should().NotBeNull();
        historicalMarketDataView.Prices.Should().NotBeNull().And.BeEmpty();
        historicalMarketDataView.MarketCaps.Should().NotBeNull().And.BeEmpty();
        historicalMarketDataView.TotalVolumes.Should().NotBeNull().And.BeEmpty();
    }
}
