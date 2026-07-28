namespace CryptoPal.Core.TokenPrice;

/// <summary>Current prices for a single token contract address.</summary>
public class ContractPrice
{
    /// <summary>Token contract address.</summary>
    public required string Address { get; init; }

    /// <summary>Latest prices per currency code.</summary>
    public required IEnumerable<Price> Prices { get; init; }
}
