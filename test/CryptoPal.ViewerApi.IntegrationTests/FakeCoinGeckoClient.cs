using CryptoPal.ApiClient.CoinGecko;
using CryptoPal.ApiClient.CoinGecko.CoinData;
using CryptoPal.ApiClient.CoinGecko.CoinHistory;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.ApiClient.CoinGecko.SimpleTokenPrice;

namespace CryptoPal.ViewerApi.IntegrationTests;

public sealed class FakeCoinGeckoClient : ICoinGeckoClient
{
    public Func<SimplePriceRequest, CancellationToken, Task<SimplePriceResponse>> GetSimplePriceHandler { get; set; } =
        (_, _) => Task.FromResult(new SimplePriceResponse
        {
            HasRequestSucceeded = true,
            CryptocurrencyPrices = new Dictionary<string, IDictionary<string, decimal>>()
        });

    public Task<SimplePriceResponse> GetSimplePriceAsync(SimplePriceRequest request, CancellationToken cancellationToken = default) =>
        GetSimplePriceHandler(request, cancellationToken);

    public Task<SimpleTokenPriceResponse> GetSimpleTokenPriceAsync(SimpleTokenPriceRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<CoinMarketChartResponse> GetCoinMarketChartAsync(CoinMarketChartRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<CoinDataResponse> GetCoinDataAsync(CoinDataRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<CoinHistoryResponse> GetCoinHistoryAsync(CoinHistoryRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}
