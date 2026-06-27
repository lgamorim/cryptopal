namespace CryptoPal.Application.CurrentPrice;

public class GetCurrentPriceQuery
{
    public required IEnumerable<string> Coins { get; init; }
    public required IEnumerable<string> Currencies { get; init; }
}
