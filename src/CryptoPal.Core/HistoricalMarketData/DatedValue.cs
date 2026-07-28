namespace CryptoPal.Core.HistoricalMarketData;

/// <summary>A decimal value on a calendar date (<c>yyyy-MM-dd</c>).</summary>
/// <param name="Date">Calendar date in invariant format.</param>
/// <param name="Value">Series value on that date.</param>
public record DatedValue(string Date, decimal Value);
