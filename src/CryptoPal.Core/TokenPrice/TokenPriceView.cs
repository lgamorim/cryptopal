namespace CryptoPal.Core.TokenPrice;

/// <summary>Result of a token price query.</summary>
/// <param name="ContractPrices">Prices for each requested contract address.</param>
public record TokenPriceView(IEnumerable<ContractPrice> ContractPrices);
