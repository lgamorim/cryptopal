using System.Net;
using System.Text;
using CryptoPal.ApiClient.CoinGecko.CoinData;
using CryptoPal.ApiClient.CoinGecko.CoinHistory;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.ApiClient.CoinGecko.SimpleTokenPrice;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace CryptoPal.ApiClient.CoinGecko.UnitTests;

public class CoinGeckoClientTests
{
    private const string TestApiKey = "test-api-key";
    [Fact]
    public async Task Should_CallGetStreamAsyncAndReturnResponseWithCorrectValues_When_SimplePriceRequestIsValid()
    {
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = new[] { "bitcoin", "ethereum", "cardano" },
            Currencies = new[] { "eur", "usd", "gbp", "jpy" }
        };
        var coinGeckoResponse = new Dictionary<string, IDictionary<string, decimal>>()
        {
            { "bitcoin", new Dictionary<string, decimal> { { "eur", 28135 }, { "usd", 30628 }, { "gbp", 24166 }, { "jpy", 4429566 } } },
            { "ethereum", new Dictionary<string, decimal> { { "eur", 1799 }, { "usd", 1958 }, { "gbp", 1545.29m }, { "jpy", 283242 } } },
            { "cardano", new Dictionary<string, decimal> { { "eur", 0.269991m }, { "usd", 0.293915m }, { "gbp", 0.231909m }, { "jpy", 42.51m } } }
        };

        var coinGeckoJsonResponse = JsonConvert.SerializeObject(coinGeckoResponse);
        var coinGeckoHttpResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(coinGeckoJsonResponse, Encoding.UTF8, "application/json")
        };

        Func<HttpRequestMessage, bool> isExpectedRequestMessage = (message) =>
        {
            const string apiArgIds = "bitcoin,ethereum,cardano";
            const string apiArgCurrencies = "eur,usd,gbp,jpy";
            const string simplePriceApiUrl = $"{CoinGeckoClient.DefaultApiBaseAddress}simple/price?ids={apiArgIds}&vs_currencies={apiArgCurrencies}";
            return message.Method.Equals(HttpMethod.Get)
                && message.RequestUri!.AbsoluteUri.Equals(simplePriceApiUrl)
                && message.Headers.Contains(CoinGeckoClient.ApiKeyHeaderName)
                && message.Headers.GetValues(CoinGeckoClient.ApiKeyHeaderName).Single().Equals(TestApiKey);
        };

        var httpClient = CreateHttpClient((message, cancellationToken) =>
        {
            isExpectedRequestMessage(message).Should().BeTrue();
            return Task.FromResult(coinGeckoHttpResponse);
        });

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var simplePriceResponse = await coinGeckoClient.GetSimplePriceAsync(simplePriceRequest, TestContext.Current.CancellationToken);

        simplePriceResponse.Should().NotBeNull();
        simplePriceResponse.HasRequestSucceeded.Should().BeTrue();
        simplePriceResponse.CryptocurrencyPrices.Should().NotBeNull().And.HaveCount(3);
        simplePriceResponse.CryptocurrencyPrices.Should().BeEquivalentTo(coinGeckoResponse);
    }

    [Fact]
    public async Task Should_GetSimplePriceThrowArgumentNullException_When_SimplePriceRequestIsNull()
    {
        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetSimplePriceAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'request')");
    }

    [Fact]
    public async Task Should_GetSimplePriceThrowArgumentNullException_When_SimplePriceRequestCurrenciesIsNull()
    {
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = new[] { "bitcoin", "ethereum", "cardano" },
            Currencies = null!
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetSimplePriceAsync(simplePriceRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.Currencies");
    }

    [Fact]
    public async Task Should_GetSimplePriceThrowArgumentNullException_When_SimplePriceRequestCoinsIsNull()
    {
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = null!,
            Currencies = new[] { "eur", "usd", "gbp", "jpy" }
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetSimplePriceAsync(simplePriceRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.Coins");
    }

    [Fact]
    public async Task Should_CallGetStreamAsyncAndReturnResponseWithCorrectValues_When_SimpleTokenPriceRequestIsValid()
    {
        var simpleTokenPriceRequest = new SimpleTokenPriceRequest()
        {
            AssetPlatformId = "ethereum",
            ContractAddresses = new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7", "0x6b175474e89094c44da98b954eedeac495271d0f" },
            Currencies = new[] { "eur", "usd" }
        };
        var coinGeckoResponse = new Dictionary<string, IDictionary<string, decimal>>()
        {
            { "0xdac17f958d2ee523a2206206994597c13d831ec7", new Dictionary<string, decimal> { { "eur", 0.92m }, { "usd", 1.0m } } },
            { "0x6b175474e89094c44da98b954eedeac495271d0f", new Dictionary<string, decimal> { { "eur", 0.919m }, { "usd", 0.999m } } }
        };

        var coinGeckoJsonResponse = JsonConvert.SerializeObject(coinGeckoResponse);
        var coinGeckoHttpResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(coinGeckoJsonResponse, Encoding.UTF8, "application/json")
        };

        Func<HttpRequestMessage, bool> isExpectedRequestMessage = (message) =>
        {
            const string apiArgId = "ethereum";
            const string apiArgContractAddresses = "0xdac17f958d2ee523a2206206994597c13d831ec7,0x6b175474e89094c44da98b954eedeac495271d0f";
            const string apiArgCurrencies = "eur,usd";
            const string simpleTokenPriceApiUrl = $"{CoinGeckoClient.DefaultApiBaseAddress}simple/token_price/{apiArgId}?contract_addresses={apiArgContractAddresses}&vs_currencies={apiArgCurrencies}";
            return message.Method.Equals(HttpMethod.Get)
                && message.RequestUri!.AbsoluteUri.Equals(simpleTokenPriceApiUrl)
                && message.Headers.Contains(CoinGeckoClient.ApiKeyHeaderName)
                && message.Headers.GetValues(CoinGeckoClient.ApiKeyHeaderName).Single().Equals(TestApiKey);
        };

        var httpClient = CreateHttpClient((message, cancellationToken) =>
        {
            isExpectedRequestMessage(message).Should().BeTrue();
            return Task.FromResult(coinGeckoHttpResponse);
        });

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var simpleTokenPriceResponse = await coinGeckoClient.GetSimpleTokenPriceAsync(simpleTokenPriceRequest, TestContext.Current.CancellationToken);

        simpleTokenPriceResponse.Should().NotBeNull();
        simpleTokenPriceResponse.HasRequestSucceeded.Should().BeTrue();
        simpleTokenPriceResponse.TokenPrices.Should().NotBeNull().And.HaveCount(2);
        simpleTokenPriceResponse.TokenPrices.Should().BeEquivalentTo(coinGeckoResponse);
    }

    [Fact]
    public async Task Should_GetSimpleTokenPriceThrowArgumentNullException_When_SimpleTokenPriceRequestIsNull()
    {
        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetSimpleTokenPriceAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'request')");
    }

    [Fact]
    public async Task Should_GetSimpleTokenPriceThrowArgumentNullException_When_SimpleTokenPriceRequestAssetPlatformIdIsNull()
    {
        var simpleTokenPriceRequest = new SimpleTokenPriceRequest()
        {
            AssetPlatformId = null!,
            ContractAddresses = new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7" },
            Currencies = new[] { "eur", "usd" }
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetSimpleTokenPriceAsync(simpleTokenPriceRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.AssetPlatformId");
    }

    [Fact]
    public async Task Should_GetSimpleTokenPriceThrowArgumentNullException_When_SimpleTokenPriceRequestContractAddressesIsNull()
    {
        var simpleTokenPriceRequest = new SimpleTokenPriceRequest()
        {
            AssetPlatformId = "ethereum",
            ContractAddresses = null!,
            Currencies = new[] { "eur", "usd" }
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetSimpleTokenPriceAsync(simpleTokenPriceRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.ContractAddresses");
    }

    [Fact]
    public async Task Should_GetSimpleTokenPriceThrowArgumentNullException_When_SimpleTokenPriceRequestCurrenciesIsNull()
    {
        var simpleTokenPriceRequest = new SimpleTokenPriceRequest()
        {
            AssetPlatformId = "ethereum",
            ContractAddresses = new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7" },
            Currencies = null!
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetSimpleTokenPriceAsync(simpleTokenPriceRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.Currencies");
    }

    [Fact]
    public async Task Should_GetSimpleTokenPriceReturnUnsuccessfulResponse_When_HttpRequestFails()
    {
        var simpleTokenPriceRequest = new SimpleTokenPriceRequest()
        {
            AssetPlatformId = "ethereum",
            ContractAddresses = new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7" },
            Currencies = new[] { "eur" }
        };

        var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var simpleTokenPriceResponse = await coinGeckoClient.GetSimpleTokenPriceAsync(simpleTokenPriceRequest, TestContext.Current.CancellationToken);

        simpleTokenPriceResponse.Should().NotBeNull();
        simpleTokenPriceResponse.HasRequestSucceeded.Should().BeFalse();
        simpleTokenPriceResponse.TokenPrices.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Should_GetSimpleTokenPriceReturnUnsuccessfulResponse_When_ResponseJsonIsInvalid()
    {
        var simpleTokenPriceRequest = new SimpleTokenPriceRequest()
        {
            AssetPlatformId = "ethereum",
            ContractAddresses = new[] { "0xdac17f958d2ee523a2206206994597c13d831ec7" },
            Currencies = new[] { "eur" }
        };

        var invalidJsonResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("this is not json", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient((_, _) => Task.FromResult(invalidJsonResponse));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var simpleTokenPriceResponse = await coinGeckoClient.GetSimpleTokenPriceAsync(simpleTokenPriceRequest, TestContext.Current.CancellationToken);

        simpleTokenPriceResponse.Should().NotBeNull();
        simpleTokenPriceResponse.HasRequestSucceeded.Should().BeFalse();
        simpleTokenPriceResponse.TokenPrices.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Should_CallGetStreamAsyncAndReturnResponseWithCorrectValues_When_CoinMarketChartRequestIsValid()
    {
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = "bitcoin",
            Currency = "eur",
            Days = 1
        };
        var coinGeckoResponse = new Dictionary<string, IList<decimal[]>>()
        {
            { "prices", new List<decimal[]> { new[] { 1688468488622m, 28477.63754439077m }, new[] { 1688554643000m, 28058.665368361602m } } },
            { "market_caps", new List<decimal[]> { new[] { 1688468488622m, 552996577247.077m }, new[] { 1688554643000m, 544504299176.5765m } } },
            { "total_volumes", new List<decimal[]> { new[] { 1688468488622m, 13732072142.597347m }, new[] { 1688554643000m, 9014016349.460764m } } }
        };

        var coinGeckoJsonResponse = JsonConvert.SerializeObject(coinGeckoResponse);
        var coinGeckoHttpResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(coinGeckoJsonResponse, Encoding.UTF8, "application/json")
        };

        Func<HttpRequestMessage, bool> isExpectedRequestMessage = (message) =>
        {
            const string argApiId = "bitcoin";
            const string argApiCurrency = "eur";
            const string argApiDays = "1";
            const string coinMarketChartApiUrl = $"{CoinGeckoClient.DefaultApiBaseAddress}coins/{argApiId}/market_chart?vs_currency={argApiCurrency}&days={argApiDays}";
            return message.Method.Equals(HttpMethod.Get)
                && message.RequestUri!.AbsoluteUri.Equals(coinMarketChartApiUrl)
                && message.Headers.Contains(CoinGeckoClient.ApiKeyHeaderName)
                && message.Headers.GetValues(CoinGeckoClient.ApiKeyHeaderName).Single().Equals(TestApiKey);
        };

        var httpClient = CreateHttpClient((message, cancellationToken) =>
        {
            isExpectedRequestMessage(message).Should().BeTrue();
            return Task.FromResult(coinGeckoHttpResponse);
        });

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinMarketChartResponse = await coinGeckoClient.GetCoinMarketChartAsync(coinMarketChartRequest, TestContext.Current.CancellationToken);

        coinMarketChartResponse.Should().NotBeNull();
        coinMarketChartResponse.HasRequestSucceeded.Should().BeTrue();
        coinMarketChartResponse.HistoricalMarketData.Should().NotBeNull();
        coinMarketChartResponse.HistoricalMarketData.Prices.Should().NotBeNull().And.HaveCount(2);
        coinMarketChartResponse.HistoricalMarketData.Prices.Should().BeEquivalentTo(ToMarketDataPoints(coinGeckoResponse["prices"]));
        coinMarketChartResponse.HistoricalMarketData.MarketCaps.Should().NotBeNull().And.HaveCount(2);
        coinMarketChartResponse.HistoricalMarketData.MarketCaps.Should().BeEquivalentTo(ToMarketDataPoints(coinGeckoResponse["market_caps"]));
        coinMarketChartResponse.HistoricalMarketData.TotalVolumes.Should().NotBeNull().And.HaveCount(2);
        coinMarketChartResponse.HistoricalMarketData.TotalVolumes.Should().BeEquivalentTo(ToMarketDataPoints(coinGeckoResponse["total_volumes"]));

        static IEnumerable<MarketDataPoint> ToMarketDataPoints(IEnumerable<decimal[]> rows) =>
            rows.Select(row => new MarketDataPoint((long)row[0], row[1]));
    }

    [Fact]
    public async Task Should_GetCoinMarketChartThrowArgumentNullException_When_CoinMarketChartRequestIsNull()
    {
        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetCoinMarketChartAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'request')");
    }

    [Fact]
    public async Task Should_GetCoinMarketChartThrowArgumentNullException_When_CoinMarketChartRequestCurrencyIsNull()
    {
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = "bitcoin",
            Currency = null!
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetCoinMarketChartAsync(coinMarketChartRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.Currency");
    }

    [Fact]
    public async Task Should_GetCoinMarketChartThrowArgumentNullException_When_CoinMarketChartRequestCoinIsNull()
    {
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = null!,
            Currency = "eur"
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetCoinMarketChartAsync(coinMarketChartRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.Coin");
    }

    [Fact]
    public async Task Should_GetSimplePriceReturnUnsuccessfulResponse_When_HttpRequestFails()
    {
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = new[] { "bitcoin" },
            Currencies = new[] { "eur" }
        };

        var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var simplePriceResponse = await coinGeckoClient.GetSimplePriceAsync(simplePriceRequest, TestContext.Current.CancellationToken);

        simplePriceResponse.Should().NotBeNull();
        simplePriceResponse.HasRequestSucceeded.Should().BeFalse();
        simplePriceResponse.CryptocurrencyPrices.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Should_GetSimplePriceReturnUnsuccessfulResponse_When_ResponseJsonIsInvalid()
    {
        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = new[] { "bitcoin" },
            Currencies = new[] { "eur" }
        };

        var invalidJsonResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("this is not json", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient((_, _) => Task.FromResult(invalidJsonResponse));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var simplePriceResponse = await coinGeckoClient.GetSimplePriceAsync(simplePriceRequest, TestContext.Current.CancellationToken);

        simplePriceResponse.Should().NotBeNull();
        simplePriceResponse.HasRequestSucceeded.Should().BeFalse();
        simplePriceResponse.CryptocurrencyPrices.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Should_GetCoinMarketChartReturnUnsuccessfulResponse_When_HttpRequestFails()
    {
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = "bitcoin",
            Currency = "eur",
            Days = 1
        };

        var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinMarketChartResponse = await coinGeckoClient.GetCoinMarketChartAsync(coinMarketChartRequest, TestContext.Current.CancellationToken);

        coinMarketChartResponse.Should().NotBeNull();
        coinMarketChartResponse.HasRequestSucceeded.Should().BeFalse();
        coinMarketChartResponse.HistoricalMarketData.Should().NotBeNull();
        coinMarketChartResponse.HistoricalMarketData.Prices.Should().BeEmpty();
        coinMarketChartResponse.HistoricalMarketData.MarketCaps.Should().BeEmpty();
        coinMarketChartResponse.HistoricalMarketData.TotalVolumes.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_GetCoinMarketChartReturnUnsuccessfulResponse_When_ResponseJsonIsInvalid()
    {
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = "bitcoin",
            Currency = "eur",
            Days = 1
        };

        var invalidJsonResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("this is not json", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient((_, _) => Task.FromResult(invalidJsonResponse));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinMarketChartResponse = await coinGeckoClient.GetCoinMarketChartAsync(coinMarketChartRequest, TestContext.Current.CancellationToken);

        coinMarketChartResponse.Should().NotBeNull();
        coinMarketChartResponse.HasRequestSucceeded.Should().BeFalse();
        coinMarketChartResponse.HistoricalMarketData.Should().NotBeNull();
        coinMarketChartResponse.HistoricalMarketData.Prices.Should().BeEmpty();
        coinMarketChartResponse.HistoricalMarketData.MarketCaps.Should().BeEmpty();
        coinMarketChartResponse.HistoricalMarketData.TotalVolumes.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_GetCoinMarketChartReturnUnsuccessfulResponse_When_MarketDataRowHasWrongElementCount()
    {
        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = "bitcoin",
            Currency = "eur",
            Days = 1
        };

        // Each market data row must be a two-element [timestamp, value] array; this one has three.
        const string malformedJson = """{"prices":[[1688468488622,28477.63,99]],"market_caps":[],"total_volumes":[]}""";
        var malformedResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(malformedJson, Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient((_, _) => Task.FromResult(malformedResponse));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinMarketChartResponse = await coinGeckoClient.GetCoinMarketChartAsync(coinMarketChartRequest, TestContext.Current.CancellationToken);

        coinMarketChartResponse.Should().NotBeNull();
        coinMarketChartResponse.HasRequestSucceeded.Should().BeFalse();
        coinMarketChartResponse.HistoricalMarketData.Prices.Should().BeEmpty();
        coinMarketChartResponse.HistoricalMarketData.MarketCaps.Should().BeEmpty();
        coinMarketChartResponse.HistoricalMarketData.TotalVolumes.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_CallGetStreamAsyncAndReturnResponseWithCorrectValues_When_CoinDataRequestIsValid()
    {
        var coinDataRequest = new CoinDataRequest()
        {
            Coin = "bitcoin"
        };
        var coinGeckoResponse = new
        {
            id = "bitcoin",
            symbol = "btc",
            name = "Bitcoin",
            description = new Dictionary<string, string> { { "en", "Bitcoin is a cryptocurrency." } },
            image = new { thumb = "thumb.png", small = "small.png", large = "large.png" },
            market_data = new
            {
                current_price = new Dictionary<string, decimal> { { "eur", 28135 }, { "usd", 30628 } },
                market_cap = new Dictionary<string, decimal> { { "eur", 552996577247m }, { "usd", 601000000000m } },
                total_volume = new Dictionary<string, decimal> { { "eur", 13732072142m }, { "usd", 15000000000m } },
                price_change_percentage_24h = 1.23m
            }
        };

        var coinGeckoJsonResponse = JsonConvert.SerializeObject(coinGeckoResponse);
        var coinGeckoHttpResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(coinGeckoJsonResponse, Encoding.UTF8, "application/json")
        };

        Func<HttpRequestMessage, bool> isExpectedRequestMessage = (message) =>
        {
            const string argApiId = "bitcoin";
            const string coinDataApiUrl = $"{CoinGeckoClient.DefaultApiBaseAddress}coins/{argApiId}?localization=false&tickers=false&market_data=true&community_data=false&developer_data=false&sparkline=false";
            return message.Method.Equals(HttpMethod.Get)
                && message.RequestUri!.AbsoluteUri.Equals(coinDataApiUrl)
                && message.Headers.Contains(CoinGeckoClient.ApiKeyHeaderName)
                && message.Headers.GetValues(CoinGeckoClient.ApiKeyHeaderName).Single().Equals(TestApiKey);
        };

        var httpClient = CreateHttpClient((message, cancellationToken) =>
        {
            isExpectedRequestMessage(message).Should().BeTrue();
            return Task.FromResult(coinGeckoHttpResponse);
        });

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinDataResponse = await coinGeckoClient.GetCoinDataAsync(coinDataRequest, TestContext.Current.CancellationToken);

        coinDataResponse.Should().NotBeNull();
        coinDataResponse.HasRequestSucceeded.Should().BeTrue();
        coinDataResponse.Coin.Should().NotBeNull();
        coinDataResponse.Coin.Id.Should().Be("bitcoin");
        coinDataResponse.Coin.Symbol.Should().Be("btc");
        coinDataResponse.Coin.Name.Should().Be("Bitcoin");
        coinDataResponse.Coin.Description.Should().NotBeNull().And.ContainKey("en");
        coinDataResponse.Coin.Description!["en"].Should().Be("Bitcoin is a cryptocurrency.");
        coinDataResponse.Coin.Image.Should().NotBeNull();
        coinDataResponse.Coin.Image!.Large.Should().Be("large.png");
        coinDataResponse.Coin.MarketData.Should().NotBeNull();
        coinDataResponse.Coin.MarketData!.CurrentPrice.Should().NotBeNull().And.HaveCount(2);
        coinDataResponse.Coin.MarketData.CurrentPrice!["eur"].Should().Be(28135);
        coinDataResponse.Coin.MarketData.MarketCap!["usd"].Should().Be(601000000000m);
        coinDataResponse.Coin.MarketData.TotalVolume!["eur"].Should().Be(13732072142m);
        coinDataResponse.Coin.MarketData.PriceChangePercentage24h.Should().Be(1.23m);
    }

    [Fact]
    public async Task Should_GetCoinDataThrowArgumentNullException_When_CoinDataRequestIsNull()
    {
        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetCoinDataAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'request')");
    }

    [Fact]
    public async Task Should_GetCoinDataThrowArgumentNullException_When_CoinDataRequestCoinIsNull()
    {
        var coinDataRequest = new CoinDataRequest()
        {
            Coin = null!
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetCoinDataAsync(coinDataRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.Coin");
    }

    [Fact]
    public async Task Should_GetCoinDataReturnUnsuccessfulResponse_When_HttpRequestFails()
    {
        var coinDataRequest = new CoinDataRequest()
        {
            Coin = "bitcoin"
        };

        var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinDataResponse = await coinGeckoClient.GetCoinDataAsync(coinDataRequest, TestContext.Current.CancellationToken);

        coinDataResponse.Should().NotBeNull();
        coinDataResponse.HasRequestSucceeded.Should().BeFalse();
        coinDataResponse.Coin.Should().NotBeNull();
        coinDataResponse.Coin.Id.Should().BeNull();
        coinDataResponse.Coin.MarketData.Should().BeNull();
    }

    [Fact]
    public async Task Should_GetCoinDataReturnUnsuccessfulResponse_When_ResponseJsonIsInvalid()
    {
        var coinDataRequest = new CoinDataRequest()
        {
            Coin = "bitcoin"
        };

        var invalidJsonResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("this is not json", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient((_, _) => Task.FromResult(invalidJsonResponse));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinDataResponse = await coinGeckoClient.GetCoinDataAsync(coinDataRequest, TestContext.Current.CancellationToken);

        coinDataResponse.Should().NotBeNull();
        coinDataResponse.HasRequestSucceeded.Should().BeFalse();
        coinDataResponse.Coin.Should().NotBeNull();
        coinDataResponse.Coin.Id.Should().BeNull();
        coinDataResponse.Coin.MarketData.Should().BeNull();
    }

    [Fact]
    public async Task Should_CallGetStreamAsyncAndReturnResponseWithCorrectValues_When_CoinHistoryRequestIsValid()
    {
        var coinHistoryRequest = new CoinHistoryRequest()
        {
            Coin = "bitcoin",
            Date = "30-12-2025"
        };
        var coinGeckoResponse = new
        {
            id = "bitcoin",
            symbol = "btc",
            name = "Bitcoin",
            image = new { thumb = "thumb.png", small = "small.png" },
            market_data = new
            {
                current_price = new Dictionary<string, decimal> { { "eur", 28135 }, { "usd", 30628 } },
                market_cap = new Dictionary<string, decimal> { { "eur", 552996577247m }, { "usd", 601000000000m } },
                total_volume = new Dictionary<string, decimal> { { "eur", 13732072142m }, { "usd", 15000000000m } }
            },
            developer_data = new
            {
                forks = 36262,
                stars = 66818,
                subscribers = 3683,
                total_issues = 7338,
                closed_issues = 7299,
                pull_requests_merged = 11215,
                pull_request_contributors = 846,
                code_additions_deletions_4_weeks = new { additions = 1101, deletions = -1480 },
                commit_count_4_weeks = 147
            }
        };

        var coinGeckoJsonResponse = JsonConvert.SerializeObject(coinGeckoResponse);
        var coinGeckoHttpResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(coinGeckoJsonResponse, Encoding.UTF8, "application/json")
        };

        Func<HttpRequestMessage, bool> isExpectedRequestMessage = (message) =>
        {
            const string argApiId = "bitcoin";
            const string argApiDate = "30-12-2025";
            const string coinHistoryApiUrl = $"{CoinGeckoClient.DefaultApiBaseAddress}coins/{argApiId}/history?date={argApiDate}&localization=false";
            return message.Method.Equals(HttpMethod.Get)
                && message.RequestUri!.AbsoluteUri.Equals(coinHistoryApiUrl)
                && message.Headers.Contains(CoinGeckoClient.ApiKeyHeaderName)
                && message.Headers.GetValues(CoinGeckoClient.ApiKeyHeaderName).Single().Equals(TestApiKey);
        };

        var httpClient = CreateHttpClient((message, cancellationToken) =>
        {
            isExpectedRequestMessage(message).Should().BeTrue();
            return Task.FromResult(coinGeckoHttpResponse);
        });

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinHistoryResponse = await coinGeckoClient.GetCoinHistoryAsync(coinHistoryRequest, TestContext.Current.CancellationToken);

        coinHistoryResponse.Should().NotBeNull();
        coinHistoryResponse.HasRequestSucceeded.Should().BeTrue();
        coinHistoryResponse.Coin.Should().NotBeNull();
        coinHistoryResponse.Coin.Id.Should().Be("bitcoin");
        coinHistoryResponse.Coin.Symbol.Should().Be("btc");
        coinHistoryResponse.Coin.Name.Should().Be("Bitcoin");
        coinHistoryResponse.Coin.Image.Should().NotBeNull();
        coinHistoryResponse.Coin.Image!.Thumb.Should().Be("thumb.png");
        coinHistoryResponse.Coin.Image.Small.Should().Be("small.png");
        coinHistoryResponse.Coin.MarketData.Should().NotBeNull();
        coinHistoryResponse.Coin.MarketData!.CurrentPrice.Should().NotBeNull().And.HaveCount(2);
        coinHistoryResponse.Coin.MarketData.CurrentPrice!["eur"].Should().Be(28135);
        coinHistoryResponse.Coin.MarketData.MarketCap!["usd"].Should().Be(601000000000m);
        coinHistoryResponse.Coin.MarketData.TotalVolume!["eur"].Should().Be(13732072142m);
        coinHistoryResponse.Coin.DeveloperData.Should().NotBeNull();
        coinHistoryResponse.Coin.DeveloperData!.Forks.Should().Be(36262);
        coinHistoryResponse.Coin.DeveloperData.Stars.Should().Be(66818);
        coinHistoryResponse.Coin.DeveloperData.Subscribers.Should().Be(3683);
        coinHistoryResponse.Coin.DeveloperData.TotalIssues.Should().Be(7338);
        coinHistoryResponse.Coin.DeveloperData.ClosedIssues.Should().Be(7299);
        coinHistoryResponse.Coin.DeveloperData.PullRequestsMerged.Should().Be(11215);
        coinHistoryResponse.Coin.DeveloperData.PullRequestContributors.Should().Be(846);
        coinHistoryResponse.Coin.DeveloperData.CodeAdditionsDeletions4Weeks.Should().NotBeNull();
        coinHistoryResponse.Coin.DeveloperData.CodeAdditionsDeletions4Weeks!.Additions.Should().Be(1101);
        coinHistoryResponse.Coin.DeveloperData.CodeAdditionsDeletions4Weeks.Deletions.Should().Be(-1480);
        coinHistoryResponse.Coin.DeveloperData.CommitCount4Weeks.Should().Be(147);
    }

    [Fact]
    public async Task Should_GetCoinHistoryThrowArgumentNullException_When_CoinHistoryRequestIsNull()
    {
        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetCoinHistoryAsync(null!, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithMessage("Value cannot be null. (Parameter 'request')");
    }

    [Fact]
    public async Task Should_GetCoinHistoryThrowArgumentNullException_When_CoinHistoryRequestCoinIsNull()
    {
        var coinHistoryRequest = new CoinHistoryRequest()
        {
            Coin = null!,
            Date = "30-12-2025"
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetCoinHistoryAsync(coinHistoryRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.Coin");
    }

    [Fact]
    public async Task Should_GetCoinHistoryThrowArgumentNullException_When_CoinHistoryRequestDateIsNull()
    {
        var coinHistoryRequest = new CoinHistoryRequest()
        {
            Coin = "bitcoin",
            Date = null!
        };

        var httpClient = CreateHttpClient();
        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);

        Func<Task> action = async () => await coinGeckoClient.GetCoinHistoryAsync(coinHistoryRequest, TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        exception.WithParameterName("request.Date");
    }

    [Fact]
    public async Task Should_GetCoinHistoryReturnUnsuccessfulResponse_When_HttpRequestFails()
    {
        var coinHistoryRequest = new CoinHistoryRequest()
        {
            Coin = "bitcoin",
            Date = "30-12-2025"
        };

        var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinHistoryResponse = await coinGeckoClient.GetCoinHistoryAsync(coinHistoryRequest, TestContext.Current.CancellationToken);

        coinHistoryResponse.Should().NotBeNull();
        coinHistoryResponse.HasRequestSucceeded.Should().BeFalse();
        coinHistoryResponse.Coin.Should().NotBeNull();
        coinHistoryResponse.Coin.Id.Should().BeNull();
        coinHistoryResponse.Coin.Image.Should().BeNull();
        coinHistoryResponse.Coin.MarketData.Should().BeNull();
        coinHistoryResponse.Coin.DeveloperData.Should().BeNull();
    }

    [Fact]
    public async Task Should_GetCoinHistoryReturnUnsuccessfulResponse_When_ResponseJsonIsInvalid()
    {
        var coinHistoryRequest = new CoinHistoryRequest()
        {
            Coin = "bitcoin",
            Date = "30-12-2025"
        };

        var invalidJsonResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("this is not json", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient((_, _) => Task.FromResult(invalidJsonResponse));

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        var coinHistoryResponse = await coinGeckoClient.GetCoinHistoryAsync(coinHistoryRequest, TestContext.Current.CancellationToken);

        coinHistoryResponse.Should().NotBeNull();
        coinHistoryResponse.HasRequestSucceeded.Should().BeFalse();
        coinHistoryResponse.Coin.Should().NotBeNull();
        coinHistoryResponse.Coin.Id.Should().BeNull();
        coinHistoryResponse.Coin.Image.Should().BeNull();
        coinHistoryResponse.Coin.MarketData.Should().BeNull();
        coinHistoryResponse.Coin.DeveloperData.Should().BeNull();
    }

    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? sendAsyncHandler = null)
    {
        var client = new HttpClient(new FakeHttpMessageHandler(sendAsyncHandler));
        CoinGeckoClient.ConfigureHttpClient(client, TestApiKey);
        return client;
    }

    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? sendAsyncHandler = null) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (sendAsyncHandler is null)
            {
                throw new InvalidOperationException("HTTP request was not expected.");
            }

            return sendAsyncHandler(request, cancellationToken);
        }
    }
}
