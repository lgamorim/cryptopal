using CryptoPal.Core;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;

namespace CryptoPal.ViewerApi;

/// <summary>Minimal REST endpoints over <see cref="ICryptocurrencyService"/>.</summary>
public static class ViewerApiEndpoints
{
    /// <summary>Maps cryptocurrency viewer routes on the application.</summary>
    public static IEndpointRouteBuilder MapViewerApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/prices", GetCurrentPriceAsync)
            .WithName("GetCurrentPrice")
            .WithSummary("Returns current prices for the requested coins and currencies.");
        endpoints.MapGet("/token-prices", GetTokenPriceAsync)
            .WithName("GetTokenPrice")
            .WithSummary("Returns current prices for token contract addresses on a platform.");
        endpoints.MapGet("/historical-market-data", GetHistoricalMarketDataAsync)
            .WithName("GetHistoricalMarketData")
            .WithSummary("Returns historical price, market cap, and volume series.");
        endpoints.MapGet("/coins/{coin}", GetCoinDataAsync)
            .WithName("GetCoinData")
            .WithSummary("Returns detailed metadata and market snapshots for a coin.");
        endpoints.MapGet("/coins/{coin}/developer-data", GetDeveloperDataAsync)
            .WithName("GetDeveloperData")
            .WithSummary("Returns developer repository activity for a coin on a historical date.");

        return endpoints;
    }

    /// <summary>Returns current prices for the requested coins and currencies.</summary>
    public static async Task<IResult> GetCurrentPriceAsync(
        ICryptocurrencyService cryptocurrencyService,
        string[] coins,
        string[] currencies,
        CancellationToken cancellationToken)
    {
        var validationError = ClientInputValidation.ValidateNonEmptyValues("coins", coins)
            ?? ClientInputValidation.ValidateNonEmptyValues("currencies", currencies);
        if (validationError is not null)
        {
            return ValidationProblemResults.BadRequest(validationError);
        }

        var query = new GetCurrentPriceQuery { Coins = coins, Currencies = currencies };
        var result = await cryptocurrencyService.GetCurrentPriceAsync(query, cancellationToken);

        return ServiceResultHttpMapper.ToHttpResult(result);
    }

    /// <summary>Returns current prices for token contract addresses on a platform.</summary>
    public static async Task<IResult> GetTokenPriceAsync(
        ICryptocurrencyService cryptocurrencyService,
        string assetPlatformId,
        string[] contractAddresses,
        string[] currencies,
        CancellationToken cancellationToken)
    {
        var validationError = ClientInputValidation.ValidateNonEmpty("assetPlatformId", assetPlatformId)
            ?? ClientInputValidation.ValidateNonEmptyValues("contractAddresses", contractAddresses)
            ?? ClientInputValidation.ValidateNonEmptyValues("currencies", currencies);
        if (validationError is not null)
        {
            return ValidationProblemResults.BadRequest(validationError);
        }

        var query = new GetTokenPriceQuery
        {
            AssetPlatformId = assetPlatformId,
            ContractAddresses = contractAddresses,
            Currencies = currencies
        };
        var result = await cryptocurrencyService.GetTokenPriceAsync(query, cancellationToken);

        return ServiceResultHttpMapper.ToHttpResult(result);
    }

    /// <summary>Returns historical price, market cap, and volume series.</summary>
    public static async Task<IResult> GetHistoricalMarketDataAsync(
        ICryptocurrencyService cryptocurrencyService,
        string coin,
        string currency,
        int days,
        CancellationToken cancellationToken)
    {
        var validationError = ClientInputValidation.ValidatePositiveDays(days);
        if (validationError is not null)
        {
            return ValidationProblemResults.BadRequest(validationError);
        }

        var query = new GetHistoricalMarketDataQuery { Coin = coin, Currency = currency, Days = days };
        var result = await cryptocurrencyService.GetHistoricalMarketDataAsync(query, cancellationToken);

        return ServiceResultHttpMapper.ToHttpResult(result);
    }

    /// <summary>Returns detailed metadata and market snapshots for a coin.</summary>
    public static async Task<IResult> GetCoinDataAsync(
        ICryptocurrencyService cryptocurrencyService,
        string coin,
        CancellationToken cancellationToken)
    {
        var query = new GetCoinDataQuery { Coin = coin };
        var result = await cryptocurrencyService.GetCoinDataAsync(query, cancellationToken);

        return ServiceResultHttpMapper.ToHttpResult(result);
    }

    /// <summary>Returns developer repository activity for a coin on a historical date.</summary>
    public static async Task<IResult> GetDeveloperDataAsync(
        ICryptocurrencyService cryptocurrencyService,
        string coin,
        string date,
        CancellationToken cancellationToken)
    {
        var validationError = ClientInputValidation.ValidateDeveloperDate(date);
        if (validationError is not null)
        {
            return ValidationProblemResults.BadRequest(validationError);
        }

        var query = new GetDeveloperDataQuery { Coin = coin, Date = date };
        var result = await cryptocurrencyService.GetDeveloperDataAsync(query, cancellationToken);

        return ServiceResultHttpMapper.ToHttpResult(result);
    }
}
