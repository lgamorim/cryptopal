using CryptoPal.ApiClient.CoinGecko;

namespace CryptoPal.ApiClient.CoinGecko.SimplePrice;

using PriceMatrix = IDictionary<string, IDictionary<string, decimal>>;

public class SimplePriceResponse : IApiResponse
{
    public bool HasRequestSucceeded { get; init; }
    public required PriceMatrix CryptocurrencyPrices { get; init; }
}
