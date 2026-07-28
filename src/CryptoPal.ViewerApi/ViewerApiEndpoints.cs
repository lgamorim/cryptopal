using CryptoPal.Core;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CryptoPal.ViewerApi;

/// <summary>Minimal REST endpoints over <see cref="ICryptocurrencyService"/>.</summary>
public static class ViewerApiEndpoints
{
    /// <summary>Maps cryptocurrency viewer routes on the application.</summary>
    public static IEndpointRouteBuilder MapViewerApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/prices", GetCurrentPriceAsync);
        endpoints.MapGet("/token-prices", GetTokenPriceAsync);
        endpoints.MapGet("/historical-market-data", GetHistoricalMarketDataAsync);
        endpoints.MapGet("/coins/{coin}", GetCoinDataAsync);
        endpoints.MapGet("/coins/{coin}/developer-data", GetDeveloperDataAsync);

        return endpoints;
    }

    /// <summary>Returns current prices for the requested coins and currencies.</summary>
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

    /// <summary>Returns current prices for token contract addresses on a platform.</summary>
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

    /// <summary>Returns historical price, market cap, and volume series.</summary>
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

    /// <summary>Returns detailed metadata and market snapshots for a coin.</summary>
    public static async Task<Ok<CoinDataView>> GetCoinDataAsync(
        ICryptocurrencyService cryptocurrencyService,
        string coin,
        CancellationToken cancellationToken)
    {
        var query = new GetCoinDataQuery { Coin = coin };
        var view = await cryptocurrencyService.GetCoinDataAsync(query, cancellationToken);

        return TypedResults.Ok(view);
    }

    /// <summary>Returns developer repository activity for a coin on a historical date.</summary>
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
