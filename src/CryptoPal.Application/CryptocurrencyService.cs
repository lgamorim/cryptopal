using System.Globalization;
using CryptoPal.ApiClient.CoinGecko;
using CryptoPal.ApiClient.CoinGecko.CoinMarketChart;
using CryptoPal.ApiClient.CoinGecko.SimplePrice;
using CryptoPal.Application.CurrentPrice;
using CryptoPal.Application.HistoricalMarketData;
using Microsoft.Extensions.Logging;

namespace CryptoPal.Application;

public class CryptocurrencyService(ICoinGeckoClient coinGeckoClient, ILogger<CryptocurrencyService> logger) : ICryptocurrencyService
{
    public async Task<CurrentPriceView> GetCurrentPriceAsync(GetCurrentPriceQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Coins);
        ArgumentNullException.ThrowIfNull(query.Currencies);

        var simplePriceRequest = new SimplePriceRequest()
        {
            Coins = query.Coins,
            Currencies = query.Currencies
        };

        IReadOnlyList<CoinPrice> coinPrices = Array.Empty<CoinPrice>();
        try
        {
            var simplePriceResponse = await coinGeckoClient.GetSimplePriceAsync(simplePriceRequest, cancellationToken);
            if (simplePriceResponse.HasRequestSucceeded)
            {
                coinPrices = MapToCoinPrices(simplePriceResponse.CryptocurrencyPrices);
            }
        }
        catch (Exception exception) when (exception is IndexOutOfRangeException or ArgumentOutOfRangeException or InvalidCastException or OverflowException)
        {
            logger.LogError(exception, "Failed to map current price response for coins {Coins}.", string.Join(',', query.Coins));
        }

        return new CurrentPriceView { CoinPrices = coinPrices };
    }

    public async Task<HistoricalMarketDataView> GetHistoricalMarketDataAsync(GetHistoricalMarketDataQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Coin);
        ArgumentNullException.ThrowIfNull(query.Currency);

        var coinMarketChartRequest = new CoinMarketChartRequest()
        {
            Coin = query.Coin,
            Currency = query.Currency,
            Days = query.Days
        };

        IList<DatedValue> prices = Array.Empty<DatedValue>();
        IList<DatedValue> marketCaps = Array.Empty<DatedValue>();
        IList<DatedValue> totalVolumes = Array.Empty<DatedValue>();
        try
        {
            var coinMarketChartResponse = await coinGeckoClient.GetCoinMarketChartAsync(coinMarketChartRequest, cancellationToken);
            if (coinMarketChartResponse.HasRequestSucceeded)
            {
                var historicalMarketData = coinMarketChartResponse.HistoricalMarketData;

                // Map all series before assigning so a mid-mapping failure leaves the view empty rather than partial.
                var mappedPrices = MapToDatedValues(historicalMarketData.Prices);
                var mappedMarketCaps = MapToDatedValues(historicalMarketData.MarketCaps);
                var mappedTotalVolumes = MapToDatedValues(historicalMarketData.TotalVolumes);

                prices = mappedPrices;
                marketCaps = mappedMarketCaps;
                totalVolumes = mappedTotalVolumes;
            }
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException)
        {
            logger.LogError(exception, "Failed to map historical market data response for coin {Coin}.", query.Coin);
        }

        return new HistoricalMarketDataView
        {
            Coin = query.Coin,
            Currency = query.Currency,
            Prices = prices,
            MarketCaps = marketCaps,
            TotalVolumes = totalVolumes
        };
    }

    private static IReadOnlyList<CoinPrice> MapToCoinPrices(IDictionary<string, IDictionary<string, decimal>> cryptoPrices)
    {
        var coinPrices = new List<CoinPrice>(cryptoPrices.Count);
        foreach (var (id, currencyPrices) in cryptoPrices)
        {
            var prices = currencyPrices.Select(pair => new Price(pair.Key, pair.Value)).ToList();
            coinPrices.Add(new CoinPrice { Id = id, Prices = prices });
        }

        return coinPrices;
    }

    private static IList<DatedValue> MapToDatedValues(IEnumerable<MarketDataPoint> points) =>
        points
            .Select(point => new DatedValue(
                DateTimeOffset.FromUnixTimeMilliseconds(point.TimestampMs).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                point.Value))
            .ToList();
}