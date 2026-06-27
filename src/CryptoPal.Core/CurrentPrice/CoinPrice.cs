namespace CryptoPal.Core.CurrentPrice;

public class CoinPrice
{
    public required string Id { get; init; }
    public required IEnumerable<Price> Prices { get; init; }
}
