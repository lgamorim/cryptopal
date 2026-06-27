using CryptoPal.Application.CurrentPrice;
using CryptoPal.Application.HistoricalMarketData;

namespace CryptoPal.Application;

public interface ICryptocurrencyService
{
    Task<CurrentPriceView> GetCurrentPriceAsync(GetCurrentPriceQuery query, CancellationToken cancellationToken = default);
    Task<HistoricalMarketDataView> GetHistoricalMarketDataAsync(GetHistoricalMarketDataQuery query, CancellationToken cancellationToken = default);
}
