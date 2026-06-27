using System.Text.Json;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using Microsoft.Extensions.Logging;

namespace CryptoPal.ApiClient.CoinGecko;

using PriceMatrix = IDictionary<string, IDictionary<string, decimal>>;

public class CoinGeckoClient(HttpClient httpClient, ILogger<CoinGeckoClient> logger) : ICoinGeckoClient
{
    public const string DefaultApiBaseAddress = "https://api.coingecko.com/api/v3/";
    public const string ApiKeyHeaderName = "x-cg-demo-api-key";

    public static void ConfigureHttpClient(HttpClient client, string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        client.BaseAddress ??= new Uri(DefaultApiBaseAddress);

        if (!client.DefaultRequestHeaders.Contains(ApiKeyHeaderName))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(ApiKeyHeaderName, apiKey);
        }
    }

    public async Task<SimplePriceResponse> GetSimplePriceAsync(SimplePriceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Coins);
        ArgumentNullException.ThrowIfNull(request.Currencies);

        const char separator = ',';
        var apiArgIds = string.Join(separator, request.Coins);
        var apiArgCurrencies = string.Join(separator, request.Currencies);
        var simplePriceApiUrl = $"simple/price?ids={apiArgIds}&vs_currencies={apiArgCurrencies}";

        SimplePriceResponse response;
        try
        {
            var resultStream = await httpClient.GetStreamAsync(simplePriceApiUrl, cancellationToken);
            var priceMatrix = await JsonSerializer.DeserializeAsync<PriceMatrix>(resultStream, cancellationToken: cancellationToken);
            response = new SimplePriceResponse
            {
                HasRequestSucceeded = true,
                CryptocurrencyPrices = priceMatrix ?? new Dictionary<string, IDictionary<string, decimal>>()
            };
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            logger.LogError(exception, "Failed to retrieve simple price from CoinGecko at {RequestUri}.", simplePriceApiUrl);
            response = new SimplePriceResponse
            {
                HasRequestSucceeded = false,
                CryptocurrencyPrices = new Dictionary<string, IDictionary<string, decimal>>()
            };
        }

        return response;
    }

    public async Task<CoinMarketChartResponse> GetCoinMarketChartAsync(CoinMarketChartRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Coin);
        ArgumentNullException.ThrowIfNull(request.Currency);

        var coinMarketChartApiUrl = $"coins/{request.Coin}/market_chart?vs_currency={request.Currency}&days={request.Days}";

        CoinMarketChartResponse response;
        try
        {
            var resultStream = await httpClient.GetStreamAsync(coinMarketChartApiUrl, cancellationToken);
            var marketChart = await JsonSerializer.DeserializeAsync<CoinMarketChartResponse.MarketChart>(resultStream, cancellationToken: cancellationToken);
            response = new CoinMarketChartResponse()
            {
                HasRequestSucceeded = true,
                HistoricalMarketData = marketChart ?? new CoinMarketChartResponse.MarketChart()
                {
                    Prices = new List<MarketDataPoint>(),
                    MarketCaps = new List<MarketDataPoint>(),
                    TotalVolumes = new List<MarketDataPoint>()
                }
            };
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            logger.LogError(exception, "Failed to retrieve coin market chart from CoinGecko at {RequestUri}.", coinMarketChartApiUrl);
            response = new CoinMarketChartResponse()
            {
                HasRequestSucceeded = false,
                HistoricalMarketData = new CoinMarketChartResponse.MarketChart()
                {
                    Prices = new List<MarketDataPoint>(),
                    MarketCaps = new List<MarketDataPoint>(),
                    TotalVolumes = new List<MarketDataPoint>()
                }
            };
        }

        return response;
    }
}