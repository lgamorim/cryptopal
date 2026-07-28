namespace CryptoPal.Core.TokenPrice;

/// <summary>Result of a token price query.</summary>
public class TokenPriceView
{
    /// <summary>Prices for each requested contract address.</summary>
    public required IEnumerable<ContractPrice> ContractPrices { get; init; }
}
