using CryptoPal.Core;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CryptoPal.ViewerApi;

public static class ViewerApiEndpoints
{
    public static IEndpointRouteBuilder MapViewerApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/prices", GetCurrentPriceAsync);
        endpoints.MapGet("/token-prices", GetTokenPriceAsync);
        endpoints.MapGet("/historical-market-data", GetHistoricalMarketDataAsync);
        endpoints.MapGet("/coins/{coin}", GetCoinDataAsync);
        endpoints.MapGet("/coins/{coin}/developer-data", GetDeveloperDataAsync);

        return endpoints;
    }

    public static async Task<Ok<CurrentPriceView>> GetCurrentPriceAsync(
        ICryptocurrencyService cryptocurrencyService,
        string[] coins,
        string[] currencies,
        CancellationToken cancellationToken)
    {
        var query = new GetCurrentPriceQuery { Coins = coins, Currencies = currencies };
        var view = await cryptocurrencyService.GetCurrentPriceAsync(query, cancellationToken);

        return TypedResults.Ok(view);
    }

    public static async Task<Ok<TokenPriceView>> GetTokenPriceAsync(
        ICryptocurrencyService cryptocurrencyService,
        string assetPlatformId,
        string[] contractAddresses,
        string[] currencies,
        CancellationToken cancellationToken)
    {
        var query = new GetTokenPriceQuery
        {
            AssetPlatformId = assetPlatformId,
            ContractAddresses = contractAddresses,
            Currencies = currencies
        };
        var view = await cryptocurrencyService.GetTokenPriceAsync(query, cancellationToken);

        return TypedResults.Ok(view);
    }

    public static async Task<Ok<HistoricalMarketDataView>> GetHistoricalMarketDataAsync(
        ICryptocurrencyService cryptocurrencyService,
        string coin,
        string currency,
        int days,
        CancellationToken cancellationToken)
    {
        var query = new GetHistoricalMarketDataQuery { Coin = coin, Currency = currency, Days = days };
        var view = await cryptocurrencyService.GetHistoricalMarketDataAsync(query, cancellationToken);

        return TypedResults.Ok(view);
    }

    public static async Task<Ok<CoinDataView>> GetCoinDataAsync(
        ICryptocurrencyService cryptocurrencyService,
        string coin,
        CancellationToken cancellationToken)
    {
        var query = new GetCoinDataQuery { Coin = coin };
        var view = await cryptocurrencyService.GetCoinDataAsync(query, cancellationToken);

        return TypedResults.Ok(view);
    }

    public static async Task<Ok<DeveloperDataView>> GetDeveloperDataAsync(
        ICryptocurrencyService cryptocurrencyService,
        string coin,
        string date,
        CancellationToken cancellationToken)
    {
        var query = new GetDeveloperDataQuery { Coin = coin, Date = date };
        var view = await cryptocurrencyService.GetDeveloperDataAsync(query, cancellationToken);

        return TypedResults.Ok(view);
    }
}
