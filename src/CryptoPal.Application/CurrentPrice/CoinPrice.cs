namespace CryptoPal.Application.CurrentPrice;

public class CoinPrice
{
    public required string Id { get; init; }
    public required IEnumerable<Price> Prices { get; init; }
}
