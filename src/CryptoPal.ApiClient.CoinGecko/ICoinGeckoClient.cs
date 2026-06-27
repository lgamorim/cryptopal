using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;

namespace CryptoPal.ApiClient.CoinGecko;

public interface ICoinGeckoClient
{
    Task<SimplePriceResponse> GetSimplePriceAsync(SimplePriceRequest request, CancellationToken cancellationToken = default);
    Task<CoinMarketChartResponse> GetCoinMarketChartAsync(CoinMarketChartRequest request, CancellationToken cancellationToken = default);
}
