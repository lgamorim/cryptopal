using CryptoPal.ApiClient.CoinGecko.CoinData;
using CryptoPal.ApiClient.CoinGecko.CoinHistory;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.ApiClient.CoinGecko.SimpleTokenPrice;

namespace CryptoPal.ApiClient.CoinGecko;

public interface ICoinGeckoClient
{
    Task<SimplePriceResponse> GetSimplePriceAsync(SimplePriceRequest request, CancellationToken cancellationToken = default);
    Task<SimpleTokenPriceResponse> GetSimpleTokenPriceAsync(SimpleTokenPriceRequest request, CancellationToken cancellationToken = default);
    Task<CoinMarketChartResponse> GetCoinMarketChartAsync(CoinMarketChartRequest request, CancellationToken cancellationToken = default);
    Task<CoinDataResponse> GetCoinDataAsync(CoinDataRequest request, CancellationToken cancellationToken = default);
    Task<CoinHistoryResponse> GetCoinHistoryAsync(CoinHistoryRequest request, CancellationToken cancellationToken = default);
}
