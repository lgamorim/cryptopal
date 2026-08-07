using System.Globalization;

namespace CryptoPal.Core;

/// <summary>Validates client-supplied query and command parameters.</summary>
public static class ClientInputValidation
{
    private const string DeveloperDateFormat = "dd-MM-yyyy";

    /// <summary>Returns an error message when <paramref name="value"/> is null or whitespace.</summary>
    public static string? ValidateNonEmpty(string parameterName, string? value) =>
        string.IsNullOrWhiteSpace(value) ? $"{parameterName} must contain at least one value." : null;

    /// <summary>Returns an error message when <paramref name="values"/> has no non-empty entries.</summary>
    public static string? ValidateNonEmptyValues(string parameterName, IEnumerable<string>? values) =>
        values is null || !values.Any(value => !string.IsNullOrWhiteSpace(value))
            ? $"{parameterName} must contain at least one value."
            : null;

    /// <summary>Returns an error message when <paramref name="days"/> is not positive.</summary>
    public static string? ValidatePositiveDays(int days) =>
        days <= 0 ? "days must be greater than zero." : null;

    /// <summary>Returns an error message when <paramref name="date"/> is not a valid <c>dd-MM-yyyy</c> date.</summary>
    public static string? ValidateDeveloperDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return "date must be a valid calendar date in dd-MM-yyyy format.";
        }

        return DateTime.TryParseExact(
            date,
            DeveloperDateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _)
            ? null
            : "date must be a valid calendar date in dd-MM-yyyy format.";
    }
}
