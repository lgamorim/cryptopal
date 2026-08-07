using System.Text.Json;
using CryptoPal.Core;
using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;

namespace CryptoPal.ViewerApp;

/// <summary>Parses CLI commands and formats cryptocurrency data for the terminal.</summary>
public sealed class ViewerAppRunner(
    ICryptocurrencyService cryptocurrencyService,
    TextWriter output,
    TextWriter error)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Runs a single command from <paramref name="args"/> and returns the process exit code.</summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>0</c> on success; <c>1</c> on usage, service failure, or runtime error.</returns>
    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            return await RunCommandAsync(args, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await error.WriteLineAsync("Operation canceled.");
            return 130;
        }
        catch (Exception exception)
        {
            await error.WriteLineAsync(exception.Message);
            return 1;
        }
    }

    private async Task<int> RunCommandAsync(string[] args, CancellationToken cancellationToken)
    {
        var jsonOutput = false;
        if (args is ["--json", .. var remainingArgs])
        {
            jsonOutput = true;
            args = remainingArgs;
        }

        switch (args)
        {
            case ["price", var coinArg, var currencyArg]:
                {
                    var coins = Split(coinArg);
                    var currencies = Split(currencyArg);
                    if (await TryWriteValidationErrorAsync(
                            ClientInputValidation.ValidateNonEmptyValues("coins", coins)
                            ?? ClientInputValidation.ValidateNonEmptyValues("currencies", currencies)))
                    {
                        return 1;
                    }

                    var query = new GetCurrentPriceQuery
                    {
                        Coins = coins,
                        Currencies = currencies
                    };
                    var result = await cryptocurrencyService.GetCurrentPriceAsync(query, cancellationToken);
                    return await HandleResult(result, async currentPrice =>
                    {
                        foreach (var coinPrice in currentPrice.CoinPrices)
                        {
                            await output.WriteLineAsync(Format(coinPrice));
                        }
                    }, jsonOutput);
                }
            case ["token", var assetPlatformArg, var addressArg, var currencyArg]:
                {
                    var contractAddresses = Split(addressArg);
                    var currencies = Split(currencyArg);
                    if (await TryWriteValidationErrorAsync(
                            ClientInputValidation.ValidateNonEmpty("assetPlatformId", assetPlatformArg)
                            ?? ClientInputValidation.ValidateNonEmptyValues("contractAddresses", contractAddresses)
                            ?? ClientInputValidation.ValidateNonEmptyValues("currencies", currencies)))
                    {
                        return 1;
                    }

                    var query = new GetTokenPriceQuery
                    {
                        AssetPlatformId = assetPlatformArg,
                        ContractAddresses = contractAddresses,
                        Currencies = currencies
                    };
                    var result = await cryptocurrencyService.GetTokenPriceAsync(query, cancellationToken);
                    return await HandleResult(result, async tokenPrice =>
                    {
                        foreach (var contractPrice in tokenPrice.ContractPrices)
                        {
                            await output.WriteLineAsync(Format(contractPrice));
                        }
                    }, jsonOutput);
                }
            case ["history", var coin, var currency, var daysArg] when int.TryParse(daysArg, out var days):
                {
                    if (await TryWriteValidationErrorAsync(ClientInputValidation.ValidatePositiveDays(days)))
                    {
                        return 1;
                    }

                    var query = new GetHistoricalMarketDataQuery { Coin = coin, Currency = currency, Days = days };
                    var result = await cryptocurrencyService.GetHistoricalMarketDataAsync(query, cancellationToken);
                    return await HandleResult(result, async historicalMarketData =>
                    {
                        await output.WriteLineAsync($"{historicalMarketData.Coin}/{historicalMarketData.Currency}");
                        foreach (var (date, value) in historicalMarketData.Prices)
                        {
                            await output.WriteLineAsync($"{date}={value}");
                        }
                    }, jsonOutput);
                }
            case ["coin", var coin]:
                {
                    var query = new GetCoinDataQuery { Coin = coin };
                    var result = await cryptocurrencyService.GetCoinDataAsync(query, cancellationToken);
                    return await HandleResult(result, async coinData =>
                    {
                        await output.WriteLineAsync($"{coinData.Id} ({coinData.Symbol}) {coinData.Name}");
                        await output.WriteLineAsync($"24h: {coinData.PriceChangePercentage24h}%");
                        foreach (var snapshot in coinData.MarketSnapshots)
                        {
                            await output.WriteLineAsync($"{snapshot.Currency}={snapshot.CurrentPrice}");
                        }
                    }, jsonOutput);
                }
            case ["developer", var coin, var date]:
                {
                    if (await TryWriteValidationErrorAsync(ClientInputValidation.ValidateDeveloperDate(date)))
                    {
                        return 1;
                    }

                    var query = new GetDeveloperDataQuery { Coin = coin, Date = date };
                    var result = await cryptocurrencyService.GetDeveloperDataAsync(query, cancellationToken);
                    return await HandleResult(result, async developerData =>
                    {
                        await output.WriteLineAsync($"{developerData.Id} ({developerData.Symbol}) {developerData.Name}");
                        await output.WriteLineAsync($"Forks: {developerData.Forks}");
                        await output.WriteLineAsync($"Stars: {developerData.Stars}");
                        await output.WriteLineAsync($"Subscribers: {developerData.Subscribers}");
                        await output.WriteLineAsync($"Total issues: {developerData.TotalIssues}");
                        await output.WriteLineAsync($"Closed issues: {developerData.ClosedIssues}");
                        await output.WriteLineAsync($"Pull requests merged: {developerData.PullRequestsMerged}");
                        await output.WriteLineAsync($"Pull request contributors: {developerData.PullRequestContributors}");
                        await output.WriteLineAsync($"Code changes (4w): +{developerData.CodeAdditions}/{developerData.CodeDeletions}");
                        await output.WriteLineAsync($"Commits (4w): {developerData.CommitCount4Weeks}");
                    }, jsonOutput);
                }
            default:
                PrintUsage();
                return 1;
        }
    }

    private async Task<bool> TryWriteValidationErrorAsync(string? validationError)
    {
        if (validationError is null)
        {
            return false;
        }

        await error.WriteLineAsync(validationError);
        return true;
    }

    private async Task<int> HandleResult<T>(ServiceResult<T> result, Func<T, Task> writeHumanReadable, bool jsonOutput)
    {
        if (!result.IsSuccess)
        {
            await error.WriteLineAsync($"{result.ErrorCode}: {result.ErrorMessage}");
            return 1;
        }

        if (jsonOutput)
        {
            await output.WriteLineAsync(JsonSerializer.Serialize(result.Value, JsonSerializerOptions));
            return 0;
        }

        await writeHumanReadable(result.Value!);
        return 0;
    }

    private static string[] Split(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Format(CoinPrice coinPrice)
    {
        var prices = string.Join(' ', coinPrice.Prices.Select(price => $"{price.Currency}={price.Value}"));
        return $"{coinPrice.Id}\n{prices}";
    }

    private static string Format(ContractPrice contractPrice)
    {
        var prices = string.Join(' ', contractPrice.Prices.Select(price => $"{price.Currency}={price.Value}"));
        return $"{contractPrice.Address}\n{prices}";
    }

    private void PrintUsage()
    {
        output.WriteLine("Usage:");
        output.WriteLine("  [--json] <command> <args...>");
        output.WriteLine("  price   <coins> <currencies>     e.g. price bitcoin,ethereum eur,usd");
        output.WriteLine("  token   <platform> <addresses> <currencies>  e.g. token ethereum 0xdac17f958d2ee523a2206206994597c13d831ec7 eur,usd");
        output.WriteLine("  history <coin> <currency> <days>  e.g. history bitcoin eur 7");
        output.WriteLine("  coin    <id>                      e.g. coin bitcoin");
        output.WriteLine("  developer <id> <date>             e.g. developer bitcoin 30-12-2025");
    }
}
