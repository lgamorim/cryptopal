namespace CryptoPal.Core.TokenPrice;

/// <summary>Current prices for a single token contract address.</summary>
/// <param name="Address">Token contract address.</param>
/// <param name="Prices">Latest prices per currency code.</param>
public record ContractPrice(string Address, IEnumerable<Price> Prices);
