using System.Net;
using System.Net.Http.Json;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.Core.CurrentPrice;
using FluentAssertions;

namespace CryptoPal.ViewerApi.IntegrationTests;

public class ViewerApiIntegrationTests : IClassFixture<ViewerApiFactory>
{
    private readonly HttpClient _client;
    private readonly FakeCoinGeckoClient _fakeCoinGeckoClient;

    public ViewerApiIntegrationTests(ViewerApiFactory factory)
    {
        _client = factory.CreateClient();
        _fakeCoinGeckoClient = factory.FakeCoinGeckoClient;
    }

    [Fact]
    public async Task Should_Return200AndJson_When_GetPricesSucceeds()
    {
        _fakeCoinGeckoClient.GetSimplePriceHandler = (_, _) => Task.FromResult(new SimplePriceResponse
        {
            HasRequestSucceeded = true,
            CryptocurrencyPrices = new Dictionary<string, IDictionary<string, decimal>>
            {
                ["bitcoin"] = new Dictionary<string, decimal> { ["eur"] = 28135m, ["usd"] = 30628m }
            }
        });

        var response = await _client.GetAsync("/prices?coins=bitcoin&currencies=eur,usd", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var currentPriceView = await response.Content.ReadFromJsonAsync<CurrentPriceView>(TestContext.Current.CancellationToken);
        currentPriceView.Should().NotBeNull();
        currentPriceView!.CoinPrices.Should().ContainSingle();
        currentPriceView.CoinPrices.Single().Id.Should().Be("bitcoin");
    }

    [Fact]
    public async Task Should_Return502ProblemDetails_When_UpstreamFails()
    {
        _fakeCoinGeckoClient.GetSimplePriceHandler = (_, _) => Task.FromResult(new SimplePriceResponse
        {
            HasRequestSucceeded = false,
            HttpStatusCode = 502,
            CryptocurrencyPrices = new Dictionary<string, IDictionary<string, decimal>>()
        });

        var response = await _client.GetAsync("/prices?coins=bitcoin&currencies=eur", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(TestContext.Current.CancellationToken);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("UpstreamUnavailable");
        problemDetails.Detail.Should().Be("Failed to retrieve prices from CoinGecko.");
    }

    [Fact]
    public async Task Should_Return400ProblemDetails_When_PricesRequestHasNoCoins()
    {
        var response = await _client.GetAsync("/prices?currencies=eur", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(TestContext.Current.CancellationToken);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("BadRequest");
        problemDetails.Detail.Should().Be("coins must contain at least one value.");
    }

    [Fact]
    public async Task Should_Return400ProblemDetails_When_HistoricalMarketDataDaysAreNotPositive()
    {
        var response = await _client.GetAsync("/historical-market-data?coin=bitcoin&currency=eur&days=0", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(TestContext.Current.CancellationToken);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("BadRequest");
        problemDetails.Detail.Should().Be("days must be greater than zero.");
    }

    [Fact]
    public async Task Should_Return400ProblemDetails_When_DeveloperDataDateIsInvalid()
    {
        var response = await _client.GetAsync("/coins/bitcoin/developer-data?date=31-02-2025", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(TestContext.Current.CancellationToken);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("BadRequest");
        problemDetails.Detail.Should().Be("date must be a valid calendar date in dd-MM-yyyy format.");
    }

    [Fact]
    public async Task Should_Return200Healthy_When_HealthEndpointRequested()
    {
        var response = await _client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var healthResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        healthResponse.Should().Be("Healthy");
    }

    [Fact]
    public async Task Should_ExposeOpenApiDocument_When_Requested()
    {
        var response = await _client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var openApiDocument = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        openApiDocument.Should().Contain("/prices");
    }

    private sealed record ProblemDetailsResponse(string? Title, string? Detail);
}
