namespace CryptoPal.Core.TokenPrice;

public class TokenPriceView
{
    public required IEnumerable<ContractPrice> ContractPrices { get; init; }
}
