using System.Net;
using System.Text;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
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
