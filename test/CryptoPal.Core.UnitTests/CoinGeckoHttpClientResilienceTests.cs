using System.Net;
using System.Net.Http.Headers;
using System.Text;
using CryptoPal.ApiClient.CoinGecko;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoPal.Core.UnitTests;

public class CoinGeckoHttpClientResilienceTests
{
    [Fact]
    public async Task Should_RetryOn429AndSucceed_When_UpstreamEventuallyReturns200()
    {
        var attempts = 0;
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient<ICoinGeckoClient, CoinGeckoClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new CountingHandler(() =>
            {
                attempts++;
                if (attempts == 1)
                {
                    var tooManyRequests = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                    tooManyRequests.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(10));
                    return tooManyRequests;
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };
            }))
            .AddCoinGeckoHttpClient("test-api-key");

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<ICoinGeckoClient>();

        var response = await client.GetSimplePriceAsync(
            new SimplePriceRequest { Coins = ["bitcoin"], Currencies = ["usd"] },
            TestContext.Current.CancellationToken);

        response.HasRequestSucceeded.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task Should_StopRetryingAfterMaxAttempts_When_UpstreamKeepsReturning429()
    {
        var attempts = 0;
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient<ICoinGeckoClient, CoinGeckoClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new CountingHandler(() =>
            {
                attempts++;
                var tooManyRequests = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                tooManyRequests.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
                return tooManyRequests;
            }))
            .AddCoinGeckoHttpClient("test-api-key");

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<ICoinGeckoClient>();

        var response = await client.GetSimplePriceAsync(
            new SimplePriceRequest { Coins = ["bitcoin"], Currencies = ["usd"] },
            TestContext.Current.CancellationToken);

        response.HasRequestSucceeded.Should().BeFalse();
        response.HttpStatusCode.Should().Be((int)HttpStatusCode.TooManyRequests);
        attempts.Should().Be(4);
    }

    private sealed class CountingHandler(Func<HttpResponseMessage> createResponse) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(createResponse());
    }
}
