namespace CryptoPal.ApiClient.CoinGecko.SimplePrice;

public class SimplePriceRequest
{
    public required IEnumerable<string> Coins { get; init; }
    public required IEnumerable<string> Currencies { get; init; }
}
