namespace CryptoPal.Core.TokenPrice;

public class ContractPrice
{
    public required string Address { get; init; }
    public required IEnumerable<Price> Prices { get; init; }
}
