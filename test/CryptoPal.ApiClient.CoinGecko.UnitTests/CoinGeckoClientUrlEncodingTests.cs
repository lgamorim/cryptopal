using System.Net;
using System.Text;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.ApiClient.CoinGecko.SimpleTokenPrice;
using CryptoPal.ApiClient.CoinGecko.CoinHistory;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace CryptoPal.ApiClient.CoinGecko.UnitTests;

public class CoinGeckoClientUrlEncodingTests
{
    private const string TestApiKey = "test-api-key";

    [Fact]
    public async Task Should_EncodeSpecialCharactersInSimplePriceUrl_When_CoinsOrCurrenciesContainReservedCharacters()
    {
        var simplePriceRequest = new SimplePriceRequest
        {
            Coins = ["bit coin", "a&b", "x,y"],
            Currencies = ["usd"]
        };

        var httpClient = CreateHttpClient((message, _) =>
        {
            message.RequestUri!.AbsoluteUri.Should().Be(
                $"{CoinGeckoClient.DefaultApiBaseAddress}simple/price?ids=bit%20coin,a%26b,x%2Cy&vs_currencies=usd");
            return Task.FromResult(CreateJsonResponse("{}"));
        });

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        await coinGeckoClient.GetSimplePriceAsync(simplePriceRequest, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_EncodeSpecialCharactersInSimpleTokenPriceUrl_When_RequestContainsReservedCharacters()
    {
        var simpleTokenPriceRequest = new SimpleTokenPriceRequest
        {
            AssetPlatformId = "eth platform",
            ContractAddresses = ["0xabc,def", "addr&1"],
            Currencies = ["us,d"]
        };

        var httpClient = CreateHttpClient((message, _) =>
        {
            message.RequestUri!.AbsoluteUri.Should().Be(
                $"{CoinGeckoClient.DefaultApiBaseAddress}simple/token_price/eth%20platform?contract_addresses=0xabc%2Cdef,addr%261&vs_currencies=us%2Cd");
            return Task.FromResult(CreateJsonResponse("{}"));
        });

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        await coinGeckoClient.GetSimpleTokenPriceAsync(simpleTokenPriceRequest, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_EncodeSpecialCharactersInCoinHistoryUrl_When_CoinOrDateContainReservedCharacters()
    {
        var coinHistoryRequest = new CoinHistoryRequest
        {
            Coin = "bit coin",
            Date = "30 12 2025"
        };

        var httpClient = CreateHttpClient((message, _) =>
        {
            message.RequestUri!.AbsoluteUri.Should().Be(
                $"{CoinGeckoClient.DefaultApiBaseAddress}coins/bit%20coin/history?date=30%2012%202025&localization=false");
            return Task.FromResult(CreateJsonResponse("{}"));
        });

        var coinGeckoClient = new CoinGeckoClient(httpClient, NullLogger<CoinGeckoClient>.Instance);
        await coinGeckoClient.GetCoinHistoryAsync(coinHistoryRequest, TestContext.Current.CancellationToken);
    }

    private static HttpResponseMessage CreateJsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncHandler)
    {
        var client = new HttpClient(new FakeHttpMessageHandler(sendAsyncHandler));
        CoinGeckoClient.ConfigureHttpClient(client, TestApiKey);
        return client;
    }

    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncHandler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            sendAsyncHandler(request, cancellationToken);
    }
}
