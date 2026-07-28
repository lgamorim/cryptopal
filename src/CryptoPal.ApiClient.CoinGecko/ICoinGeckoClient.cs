using CryptoPal.ApiClient.CoinGecko.CoinData;
using CryptoPal.ApiClient.CoinGecko.CoinHistory;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.ApiClient.CoinGecko.SimpleTokenPrice;

namespace CryptoPal.ApiClient.CoinGecko;

/// <summary>
/// Client for the CoinGecko REST API.
/// </summary>
public interface ICoinGeckoClient
{
    /// <summary>Gets simple prices for coin IDs.</summary>
    Task<SimplePriceResponse> GetSimplePriceAsync(SimplePriceRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets simple prices for token contract addresses on a platform.</summary>
    Task<SimpleTokenPriceResponse> GetSimpleTokenPriceAsync(SimpleTokenPriceRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets historical market chart data for a coin.</summary>
    Task<CoinMarketChartResponse> GetCoinMarketChartAsync(CoinMarketChartRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets detailed coin data by ID.</summary>
    Task<CoinDataResponse> GetCoinDataAsync(CoinDataRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets coin snapshot data for a historical date.</summary>
    Task<CoinHistoryResponse> GetCoinHistoryAsync(CoinHistoryRequest request, CancellationToken cancellationToken = default);
}
