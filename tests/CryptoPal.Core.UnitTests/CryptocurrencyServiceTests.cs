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
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CryptoPal.Core.UnitTests;

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
    public async Task Should_CallGetSimpleTokenPriceAndReturnViewWithCorrectMapping_When_GetTokenPriceQueryIsValid()
    {
        var getTokenPriceQuery = new GetTokenPriceQuery()
        {
            AssetPlatformId = "ethereum",
            ContractAddresses = new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7", "0x6b175474e89094c44da98b954eedeac495271d0f" },
            Currencies = new[] { "eur", "usd" }
        };
        var simpleTokenPriceRequest = new SimpleTokenPriceRequest()
        {
            AssetPlatformId = getTokenPriceQuery.AssetPlatformId,
            ContractAddresses = getTokenPriceQuery.ContractAddresses,
            Currencies = getTokenPriceQuery.Currencies
        };
        var simpleTokenPriceResponse = new SimpleTokenPriceResponse()
        {
            HasRequestSucceeded = true,
            TokenPrices = new Dictionary<string, IDictionary<string, decimal>>()
            {
                { "0xdac17f958d2ee523a2206206994597c13d831ec7", new Dictionary<string, decimal> { { "eur", 0.92m }, { "usd", 1.0m } } },
                { "0x6b175474e89094c44da98b954eedeac495271d0f", new Dictionary<string, decimal> { { "eur", 0.919m }, { "usd", 0.999m } } }
            }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetSimpleTokenPriceAsync(Arg.Any<SimpleTokenPriceRequest>(), Arg.Any<CancellationToken>())
            .Returns(simpleTokenPriceResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var tokenPriceView = await cryptocurrencyService.GetTokenPriceAsync(getTokenPriceQuery, TestContext.Current.CancellationToken);

        Func<SimpleTokenPriceRequest, SimpleTokenPriceRequest, bool> isExpectedRequest = (request, expected) =>
            request.AssetPlatformId == expected.AssetPlatformId
            && Equals(request.ContractAddresses, expected.ContractAddresses)
            && Equals(request.Currencies, expected.Currencies);
        await coinGeckoClient.Received(1).GetSimpleTokenPriceAsync(Arg.Is<SimpleTokenPriceRequest>(request => isExpectedRequest(request, simpleTokenPriceRequest)), Arg.Any<CancellationToken>());

        tokenPriceView.Should().NotBeNull();
        tokenPriceView.ContractPrices.Should().NotBeNull();
        tokenPriceView.ContractPrices.Count().Should().Be(2);
        AssertTokenPriceViewContractPrices(tokenPriceView.ContractPrices);

        void AssertTokenPriceViewContractPrices(IEnumerable<ContractPrice> contractPrices)
        {
            var sampledTokenPrices = simpleTokenPriceResponse.TokenPrices;
            foreach (var contractPrice in contractPrices)
            {
                sampledTokenPrices.Should().ContainKey(contractPrice.Address);
                var sampledContractPrice = sampledTokenPrices[contractPrice.Address];
                foreach (var price in contractPrice.Prices)
                {
                    sampledContractPrice.Should().ContainKey(price.Currency);
                    price.Value.Should().Be(sampledContractPrice[price.Currency]);
                }
            }
        }
    }

    [Fact]
    public async Task Should_GetTokenPriceThrowArgumentNullException_When_GetTokenPriceQueryIsNull()
    {
        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetTokenPriceAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'query')");
    }

    [Fact]
    public async Task Should_GetTokenPriceThrowArgumentNullException_When_GetTokenPriceQueryAssetPlatformIdIsNull()
    {
        var getTokenPriceQuery = new GetTokenPriceQuery()
        {
            AssetPlatformId = null!,
            ContractAddresses = new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7" },
            Currencies = new[] { "eur", "usd" }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetTokenPriceAsync(getTokenPriceQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.AssetPlatformId");
    }

    [Fact]
    public async Task Should_GetTokenPriceThrowArgumentNullException_When_GetTokenPriceQueryContractAddressesIsNull()
    {
        var getTokenPriceQuery = new GetTokenPriceQuery()
        {
            AssetPlatformId = "ethereum",
            ContractAddresses = null!,
            Currencies = new[] { "eur", "usd" }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetTokenPriceAsync(getTokenPriceQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.ContractAddresses");
    }

    [Fact]
    public async Task Should_GetTokenPriceThrowArgumentNullException_When_GetTokenPriceQueryCurrenciesIsNull()
    {
        var getTokenPriceQuery = new GetTokenPriceQuery()
        {
            AssetPlatformId = "ethereum",
            ContractAddresses = new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7" },
            Currencies = null!
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetTokenPriceAsync(getTokenPriceQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.Currencies");
    }

    [Fact]
    public async Task Should_GetTokenPriceReturnEmptyView_When_SimpleTokenPriceResponseIsUnsuccessful()
    {
        var getTokenPriceQuery = new GetTokenPriceQuery()
        {
            AssetPlatformId = "ethereum",
            ContractAddresses = new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7" },
            Currencies = new[] { "eur" }
        };
        var simpleTokenPriceResponse = new SimpleTokenPriceResponse()
        {
            HasRequestSucceeded = false,
            TokenPrices = new Dictionary<string, IDictionary<string, decimal>>()
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetSimpleTokenPriceAsync(Arg.Any<SimpleTokenPriceRequest>(), Arg.Any<CancellationToken>())
            .Returns(simpleTokenPriceResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var tokenPriceView = await cryptocurrencyService.GetTokenPriceAsync(getTokenPriceQuery, TestContext.Current.CancellationToken);

        tokenPriceView.Should().NotBeNull();
        tokenPriceView.ContractPrices.Should().NotBeNull().And.BeEmpty();
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

    [Fact]
    public async Task Should_CallGetCoinDataAndReturnViewWithCorrectMapping_When_GetCoinDataQueryIsValid()
    {
        var getCoinDataQuery = new GetCoinDataQuery()
        {
            Coin = "bitcoin"
        };
        var coinDataResponse = new CoinDataResponse()
        {
            HasRequestSucceeded = true,
            Coin = new CoinDataResponse.CoinDetail()
            {
                Id = "bitcoin",
                Symbol = "btc",
                Name = "Bitcoin",
                Description = new Dictionary<string, string> { { "en", "Bitcoin is a cryptocurrency." } },
                Image = new CoinDataResponse.CoinImage { Thumb = "thumb.png", Small = "small.png", Large = "large.png" },
                MarketData = new CoinDataResponse.CoinMarketData
                {
                    CurrentPrice = new Dictionary<string, decimal> { { "eur", 28135 }, { "usd", 30628 } },
                    MarketCap = new Dictionary<string, decimal> { { "eur", 552996577247m }, { "usd", 601000000000m } },
                    TotalVolume = new Dictionary<string, decimal> { { "eur", 13732072142m }, { "usd", 15000000000m } },
                    PriceChangePercentage24h = 1.23m
                }
            }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinDataAsync(Arg.Any<CoinDataRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinDataResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var coinDataView = await cryptocurrencyService.GetCoinDataAsync(getCoinDataQuery, TestContext.Current.CancellationToken);

        await coinGeckoClient.Received(1).GetCoinDataAsync(Arg.Is<CoinDataRequest>(request => request.Coin == getCoinDataQuery.Coin), Arg.Any<CancellationToken>());

        coinDataView.Should().NotBeNull();
        coinDataView.Id.Should().Be("bitcoin");
        coinDataView.Symbol.Should().Be("btc");
        coinDataView.Name.Should().Be("Bitcoin");
        coinDataView.Description.Should().Be("Bitcoin is a cryptocurrency.");
        coinDataView.ImageUrl.Should().Be("large.png");
        coinDataView.PriceChangePercentage24h.Should().Be(1.23m);
        coinDataView.MarketSnapshots.Should().BeEquivalentTo(new[]
        {
            new CoinMarketSnapshot("eur", 28135, 552996577247m, 13732072142m),
            new CoinMarketSnapshot("usd", 30628, 601000000000m, 15000000000m)
        });
    }

    [Fact]
    public async Task Should_CallGetCoinDataThrowArgumentNullException_When_GetCoinDataQueryIsNull()
    {
        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetCoinDataAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'query')");
    }

    [Fact]
    public async Task Should_CallGetCoinDataThrowArgumentNullException_When_GetCoinDataQueryCoinIsNull()
    {
        var getCoinDataQuery = new GetCoinDataQuery()
        {
            Coin = null!
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetCoinDataAsync(getCoinDataQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.Coin");
    }

    [Fact]
    public async Task Should_GetCoinDataReturnEmptyView_When_CoinDataResponseIsUnsuccessful()
    {
        var getCoinDataQuery = new GetCoinDataQuery()
        {
            Coin = "bitcoin"
        };
        var coinDataResponse = new CoinDataResponse()
        {
            HasRequestSucceeded = false,
            Coin = new CoinDataResponse.CoinDetail()
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinDataAsync(Arg.Any<CoinDataRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinDataResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var coinDataView = await cryptocurrencyService.GetCoinDataAsync(getCoinDataQuery, TestContext.Current.CancellationToken);

        coinDataView.Should().NotBeNull();
        coinDataView.Id.Should().Be("bitcoin");
        coinDataView.Symbol.Should().BeEmpty();
        coinDataView.Name.Should().BeEmpty();
        coinDataView.Description.Should().BeEmpty();
        coinDataView.ImageUrl.Should().BeEmpty();
        coinDataView.PriceChangePercentage24h.Should().Be(0);
        coinDataView.MarketSnapshots.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Should_GetCoinDataReturnViewWithDefaults_When_ResponseHasNullMarketDataAndDescription()
    {
        var getCoinDataQuery = new GetCoinDataQuery()
        {
            Coin = "bitcoin"
        };
        var coinDataResponse = new CoinDataResponse()
        {
            HasRequestSucceeded = true,
            Coin = new CoinDataResponse.CoinDetail()
            {
                Id = "bitcoin",
                Symbol = "btc",
                Name = "Bitcoin",
                Description = null,
                Image = null,
                MarketData = null
            }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinDataAsync(Arg.Any<CoinDataRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinDataResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var coinDataView = await cryptocurrencyService.GetCoinDataAsync(getCoinDataQuery, TestContext.Current.CancellationToken);

        coinDataView.Should().NotBeNull();
        coinDataView.Id.Should().Be("bitcoin");
        coinDataView.Symbol.Should().Be("btc");
        coinDataView.Name.Should().Be("Bitcoin");
        coinDataView.Description.Should().BeEmpty();
        coinDataView.ImageUrl.Should().BeEmpty();
        coinDataView.PriceChangePercentage24h.Should().Be(0);
        coinDataView.MarketSnapshots.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Should_CallGetDeveloperDataAndReturnViewWithCorrectMapping_When_GetDeveloperDataQueryIsValid()
    {
        var getDeveloperDataQuery = new GetDeveloperDataQuery()
        {
            Coin = "bitcoin",
            Date = "30-12-2025"
        };
        var coinHistoryResponse = new CoinHistoryResponse()
        {
            HasRequestSucceeded = true,
            Coin = new CoinHistoryResponse.CoinHistoryDetail()
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

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinHistoryAsync(Arg.Any<CoinHistoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinHistoryResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var developerDataView = await cryptocurrencyService.GetDeveloperDataAsync(getDeveloperDataQuery, TestContext.Current.CancellationToken);

        await coinGeckoClient.Received(1).GetCoinHistoryAsync(
            Arg.Is<CoinHistoryRequest>(request => request.Coin == getDeveloperDataQuery.Coin && request.Date == getDeveloperDataQuery.Date),
            Arg.Any<CancellationToken>());

        developerDataView.Should().NotBeNull();
        developerDataView.Id.Should().Be("bitcoin");
        developerDataView.Symbol.Should().Be("btc");
        developerDataView.Name.Should().Be("Bitcoin");
        developerDataView.Forks.Should().Be(36262);
        developerDataView.Stars.Should().Be(66818);
        developerDataView.Subscribers.Should().Be(3683);
        developerDataView.TotalIssues.Should().Be(7338);
        developerDataView.ClosedIssues.Should().Be(7299);
        developerDataView.PullRequestsMerged.Should().Be(11215);
        developerDataView.PullRequestContributors.Should().Be(846);
        developerDataView.CodeAdditions.Should().Be(1101);
        developerDataView.CodeDeletions.Should().Be(-1480);
        developerDataView.CommitCount4Weeks.Should().Be(147);
    }

    [Fact]
    public async Task Should_CallGetDeveloperDataThrowArgumentNullException_When_GetDeveloperDataQueryIsNull()
    {
        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetDeveloperDataAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'query')");
    }

    [Fact]
    public async Task Should_CallGetDeveloperDataThrowArgumentNullException_When_GetDeveloperDataQueryCoinIsNull()
    {
        var getDeveloperDataQuery = new GetDeveloperDataQuery()
        {
            Coin = null!,
            Date = "30-12-2025"
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetDeveloperDataAsync(getDeveloperDataQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.Coin");
    }

    [Fact]
    public async Task Should_CallGetDeveloperDataThrowArgumentNullException_When_GetDeveloperDataQueryDateIsNull()
    {
        var getDeveloperDataQuery = new GetDeveloperDataQuery()
        {
            Coin = "bitcoin",
            Date = null!
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);

        Func<Task> action = async () => await cryptocurrencyService.GetDeveloperDataAsync(getDeveloperDataQuery, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("query.Date");
    }

    [Fact]
    public async Task Should_GetDeveloperDataReturnEmptyView_When_DeveloperDataResponseIsUnsuccessful()
    {
        var getDeveloperDataQuery = new GetDeveloperDataQuery()
        {
            Coin = "bitcoin",
            Date = "30-12-2025"
        };
        var coinHistoryResponse = new CoinHistoryResponse()
        {
            HasRequestSucceeded = false,
            Coin = new CoinHistoryResponse.CoinHistoryDetail()
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinHistoryAsync(Arg.Any<CoinHistoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinHistoryResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var developerDataView = await cryptocurrencyService.GetDeveloperDataAsync(getDeveloperDataQuery, TestContext.Current.CancellationToken);

        developerDataView.Should().NotBeNull();
        developerDataView.Id.Should().Be("bitcoin");
        developerDataView.Symbol.Should().BeEmpty();
        developerDataView.Name.Should().BeEmpty();
        developerDataView.Forks.Should().Be(0);
        developerDataView.Stars.Should().Be(0);
        developerDataView.Subscribers.Should().Be(0);
        developerDataView.TotalIssues.Should().Be(0);
        developerDataView.ClosedIssues.Should().Be(0);
        developerDataView.PullRequestsMerged.Should().Be(0);
        developerDataView.PullRequestContributors.Should().Be(0);
        developerDataView.CodeAdditions.Should().Be(0);
        developerDataView.CodeDeletions.Should().Be(0);
        developerDataView.CommitCount4Weeks.Should().Be(0);
    }

    [Fact]
    public async Task Should_GetDeveloperDataReturnViewWithDefaults_When_ResponseHasNullDeveloperData()
    {
        var getDeveloperDataQuery = new GetDeveloperDataQuery()
        {
            Coin = "bitcoin",
            Date = "30-12-2025"
        };
        var coinHistoryResponse = new CoinHistoryResponse()
        {
            HasRequestSucceeded = true,
            Coin = new CoinHistoryResponse.CoinHistoryDetail()
            {
                Id = "bitcoin",
                Symbol = "btc",
                Name = "Bitcoin",
                DeveloperData = null
            }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinHistoryAsync(Arg.Any<CoinHistoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinHistoryResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var developerDataView = await cryptocurrencyService.GetDeveloperDataAsync(getDeveloperDataQuery, TestContext.Current.CancellationToken);

        developerDataView.Should().NotBeNull();
        developerDataView.Id.Should().Be("bitcoin");
        developerDataView.Symbol.Should().Be("btc");
        developerDataView.Name.Should().Be("Bitcoin");
        developerDataView.Forks.Should().Be(0);
        developerDataView.Stars.Should().Be(0);
        developerDataView.Subscribers.Should().Be(0);
        developerDataView.TotalIssues.Should().Be(0);
        developerDataView.ClosedIssues.Should().Be(0);
        developerDataView.PullRequestsMerged.Should().Be(0);
        developerDataView.PullRequestContributors.Should().Be(0);
        developerDataView.CodeAdditions.Should().Be(0);
        developerDataView.CodeDeletions.Should().Be(0);
        developerDataView.CommitCount4Weeks.Should().Be(0);
    }

    [Fact]
    public async Task Should_GetDeveloperDataReturnViewWithDefaultCodeChanges_When_ResponseHasNullCodeAdditionsDeletions()
    {
        var getDeveloperDataQuery = new GetDeveloperDataQuery()
        {
            Coin = "bitcoin",
            Date = "30-12-2025"
        };
        var coinHistoryResponse = new CoinHistoryResponse()
        {
            HasRequestSucceeded = true,
            Coin = new CoinHistoryResponse.CoinHistoryDetail()
            {
                Id = "bitcoin",
                Symbol = "btc",
                Name = "Bitcoin",
                DeveloperData = new CoinHistoryResponse.CoinDeveloperData
                {
                    Forks = 36262,
                    CodeAdditionsDeletions4Weeks = null,
                    CommitCount4Weeks = null
                }
            }
        };

        var coinGeckoClient = Substitute.For<ICoinGeckoClient>();
        coinGeckoClient.GetCoinHistoryAsync(Arg.Any<CoinHistoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(coinHistoryResponse);

        var cryptocurrencyService = new CryptocurrencyService(coinGeckoClient, NullLogger<CryptocurrencyService>.Instance);
        var developerDataView = await cryptocurrencyService.GetDeveloperDataAsync(getDeveloperDataQuery, TestContext.Current.CancellationToken);

        developerDataView.Should().NotBeNull();
        developerDataView.Forks.Should().Be(36262);
        developerDataView.CodeAdditions.Should().Be(0);
        developerDataView.CodeDeletions.Should().Be(0);
        developerDataView.CommitCount4Weeks.Should().Be(0);
    }
}
