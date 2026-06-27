using CryptoPal.Application;
using CryptoPal.Application.CurrentPrice;
using CryptoPal.Application.HistoricalMarketData;

namespace CryptoPal.ViewerApp;

public sealed class ViewerAppRunner(
    ICryptocurrencyService cryptocurrencyService,
    TextWriter output,
    TextWriter error)
{
    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            return await RunCommandAsync(args, cancellationToken);
        }
        catch (Exception exception)
        {
            await error.WriteLineAsync(exception.Message);
            return 1;
        }
    }

    private async Task<int> RunCommandAsync(string[] args, CancellationToken cancellationToken)
    {
        switch (args)
        {
            case ["price", var coinArg, var currencyArg]:
            {
                var query = new GetCurrentPriceQuery
                {
                    Coins = Split(coinArg),
                    Currencies = Split(currencyArg)
                };
                var currentPrice = await cryptocurrencyService.GetCurrentPriceAsync(query, cancellationToken);
                foreach (var coinPrice in currentPrice.CoinPrices)
                {
                    await output.WriteLineAsync(Format(coinPrice));
                }

                return 0;
            }
            case ["history", var coin, var currency, var daysArg] when int.TryParse(daysArg, out var days):
            {
                var query = new GetHistoricalMarketDataQuery { Coin = coin, Currency = currency, Days = days };
                var historicalMarketData = await cryptocurrencyService.GetHistoricalMarketDataAsync(query, cancellationToken);
                await output.WriteLineAsync($"{historicalMarketData.Coin}/{historicalMarketData.Currency}");
                foreach (var (date, value) in historicalMarketData.Prices)
                {
                    await output.WriteLineAsync($"{date}={value}");
                }

                return 0;
            }
            default:
                PrintUsage();
                return 1;
        }
    }

    private static string[] Split(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Format(CoinPrice coinPrice)
    {
        var prices = string.Join(' ', coinPrice.Prices.Select(price => $"{price.Currency}={price.Value}"));
        return $"{coinPrice.Id}\n{prices}";
    }

    private void PrintUsage()
    {
        output.WriteLine("Usage:");
        output.WriteLine("  price   <coins> <currencies>     e.g. price bitcoin,ethereum eur,usd");
        output.WriteLine("  history <coin> <currency> <days>  e.g. history bitcoin eur 7");
    }
}
