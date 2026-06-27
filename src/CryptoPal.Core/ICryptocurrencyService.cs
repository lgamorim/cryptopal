using CryptoPal.Core.CoinData;
using CryptoPal.Core.CurrentPrice;
using CryptoPal.Core.DeveloperData;
using CryptoPal.Core.HistoricalMarketData;
using CryptoPal.Core.TokenPrice;

namespace CryptoPal.Core;

public interface ICryptocurrencyService
{
    Task<CurrentPriceView> GetCurrentPriceAsync(GetCurrentPriceQuery query, CancellationToken cancellationToken = default);
    Task<TokenPriceView> GetTokenPriceAsync(GetTokenPriceQuery query, CancellationToken cancellationToken = default);
    Task<HistoricalMarketDataView> GetHistoricalMarketDataAsync(GetHistoricalMarketDataQuery query, CancellationToken cancellationToken = default);
    Task<CoinDataView> GetCoinDataAsync(GetCoinDataQuery query, CancellationToken cancellationToken = default);
    Task<DeveloperDataView> GetDeveloperDataAsync(GetDeveloperDataQuery query, CancellationToken cancellationToken = default);
}
