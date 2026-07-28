namespace CryptoPal.Core;

/// <summary>A quoted value in a specific fiat or crypto currency.</summary>
/// <param name="Currency">Currency code.</param>
/// <param name="Value">Quoted amount.</param>
public record Price(string Currency, decimal Value);
