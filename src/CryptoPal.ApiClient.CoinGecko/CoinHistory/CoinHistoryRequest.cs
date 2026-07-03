namespace CryptoPal.ApiClient.CoinGecko.CoinHistory;

public class CoinHistoryRequest
{
    public required string Coin { get; init; }
    public required string Date { get; init; }
}
