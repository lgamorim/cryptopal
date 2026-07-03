namespace CryptoPal.ApiClient.CoinGecko.CoinMarketChart;

public class CoinMarketChartRequest
{
    public required string Coin { get; init; }
    public required string Currency { get; init; }
    public int Days { get; init; }
}
