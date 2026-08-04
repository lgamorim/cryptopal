using System.Text.Json;
using CryptoPal.ApiClient.CoinGecko.CoinData;
using CryptoPal.ApiClient.CoinGecko.CoinHistory;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.ApiClient.CoinGecko.SimpleTokenPrice;
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

        try
        {
            var resultStream = await httpClient.GetStreamAsync(simplePriceApiUrl, cancellationToken);
            var priceMatrix = await JsonSerializer.DeserializeAsync<PriceMatrix>(resultStream, cancellationToken: cancellationToken);
            return new SimplePriceResponse
            {
                HasRequestSucceeded = true,
                CryptocurrencyPrices = priceMatrix ?? new Dictionary<string, IDictionary<string, decimal>>()
            };
        }
        catch (Exception exception) when (ShouldHandleAsFailedRequest(exception, cancellationToken))
        {
            LogRequestFailure(exception, simplePriceApiUrl);
            return new SimplePriceResponse
            {
                HasRequestSucceeded = false,
                HttpStatusCode = GetHttpStatusCode(exception),
                CryptocurrencyPrices = new Dictionary<string, IDictionary<string, decimal>>()
            };
        }
    }

    public async Task<SimpleTokenPriceResponse> GetSimpleTokenPriceAsync(SimpleTokenPriceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.AssetPlatformId);
        ArgumentNullException.ThrowIfNull(request.ContractAddresses);
        ArgumentNullException.ThrowIfNull(request.Currencies);

        const char separator = ',';
        var apiArgContractAddresses = string.Join(separator, request.ContractAddresses);
        var apiArgCurrencies = string.Join(separator, request.Currencies);
        var simpleTokenPriceApiUrl = $"simple/token_price/{request.AssetPlatformId}?contract_addresses={apiArgContractAddresses}&vs_currencies={apiArgCurrencies}";

        try
        {
            var resultStream = await httpClient.GetStreamAsync(simpleTokenPriceApiUrl, cancellationToken);
            var priceMatrix = await JsonSerializer.DeserializeAsync<PriceMatrix>(resultStream, cancellationToken: cancellationToken);
            return new SimpleTokenPriceResponse
            {
                HasRequestSucceeded = true,
                TokenPrices = priceMatrix ?? new Dictionary<string, IDictionary<string, decimal>>()
            };
        }
        catch (Exception exception) when (ShouldHandleAsFailedRequest(exception, cancellationToken))
        {
            LogRequestFailure(exception, simpleTokenPriceApiUrl);
            return new SimpleTokenPriceResponse
            {
                HasRequestSucceeded = false,
                HttpStatusCode = GetHttpStatusCode(exception),
                TokenPrices = new Dictionary<string, IDictionary<string, decimal>>()
            };
        }
    }

    public async Task<CoinMarketChartResponse> GetCoinMarketChartAsync(CoinMarketChartRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Coin);
        ArgumentNullException.ThrowIfNull(request.Currency);

        var coinMarketChartApiUrl = $"coins/{request.Coin}/market_chart?vs_currency={request.Currency}&days={request.Days}";

        try
        {
            var resultStream = await httpClient.GetStreamAsync(coinMarketChartApiUrl, cancellationToken);
            var marketChart = await JsonSerializer.DeserializeAsync<CoinMarketChartResponse.MarketChart>(resultStream, cancellationToken: cancellationToken);
            return new CoinMarketChartResponse()
            {
                HasRequestSucceeded = true,
                HistoricalMarketData = marketChart ?? CreateEmptyMarketChart()
            };
        }
        catch (Exception exception) when (ShouldHandleAsFailedRequest(exception, cancellationToken))
        {
            LogRequestFailure(exception, coinMarketChartApiUrl);
            return new CoinMarketChartResponse()
            {
                HasRequestSucceeded = false,
                HttpStatusCode = GetHttpStatusCode(exception),
                HistoricalMarketData = CreateEmptyMarketChart()
            };
        }
    }

    public async Task<CoinDataResponse> GetCoinDataAsync(CoinDataRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Coin);

        var coinDataApiUrl = $"coins/{request.Coin}?localization=false&tickers=false&market_data=true&community_data=false&developer_data=false&sparkline=false";

        try
        {
            var resultStream = await httpClient.GetStreamAsync(coinDataApiUrl, cancellationToken);
            var coinDetail = await JsonSerializer.DeserializeAsync<CoinDataResponse.CoinDetail>(resultStream, cancellationToken: cancellationToken);
            return new CoinDataResponse()
            {
                HasRequestSucceeded = true,
                Coin = coinDetail ?? new CoinDataResponse.CoinDetail()
            };
        }
        catch (Exception exception) when (ShouldHandleAsFailedRequest(exception, cancellationToken))
        {
            LogRequestFailure(exception, coinDataApiUrl);
            return new CoinDataResponse()
            {
                HasRequestSucceeded = false,
                HttpStatusCode = GetHttpStatusCode(exception),
                Coin = new CoinDataResponse.CoinDetail()
            };
        }
    }

    public async Task<CoinHistoryResponse> GetCoinHistoryAsync(CoinHistoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Coin);
        ArgumentNullException.ThrowIfNull(request.Date);

        var coinHistoryApiUrl = $"coins/{request.Coin}/history?date={request.Date}&localization=false";

        try
        {
            var resultStream = await httpClient.GetStreamAsync(coinHistoryApiUrl, cancellationToken);
            var coinHistory = await JsonSerializer.DeserializeAsync<CoinHistoryResponse.CoinHistoryDetail>(resultStream, cancellationToken: cancellationToken);
            return new CoinHistoryResponse()
            {
                HasRequestSucceeded = true,
                Coin = coinHistory ?? new CoinHistoryResponse.CoinHistoryDetail()
            };
        }
        catch (Exception exception) when (ShouldHandleAsFailedRequest(exception, cancellationToken))
        {
            LogRequestFailure(exception, coinHistoryApiUrl);
            return new CoinHistoryResponse()
            {
                HasRequestSucceeded = false,
                HttpStatusCode = GetHttpStatusCode(exception),
                Coin = new CoinHistoryResponse.CoinHistoryDetail()
            };
        }
    }

    private static CoinMarketChartResponse.MarketChart CreateEmptyMarketChart() =>
        new()
        {
            Prices = new List<MarketDataPoint>(),
            MarketCaps = new List<MarketDataPoint>(),
            TotalVolumes = new List<MarketDataPoint>()
        };

    private static bool ShouldHandleAsFailedRequest(Exception exception, CancellationToken cancellationToken) =>
        exception is HttpRequestException or JsonException
        || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested);

    private static int? GetHttpStatusCode(Exception exception) =>
        exception is HttpRequestException { StatusCode: { } status } ? (int)status : null;

    private void LogRequestFailure(Exception exception, string requestUri) =>
        logger.LogError(exception, "Failed to retrieve data from CoinGecko at {RequestUri}.", requestUri);
}
