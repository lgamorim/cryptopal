using CryptoPal.ApiClient.CoinGecko;

namespace CryptoPal.ApiClient.CoinGecko.SimpleTokenPrice;

using PriceMatrix = IDictionary<string, IDictionary<string, decimal>>;

public class SimpleTokenPriceResponse : IApiResponse
{
    public bool HasRequestSucceeded { get; init; }
    public required PriceMatrix TokenPrices { get; init; }
}
