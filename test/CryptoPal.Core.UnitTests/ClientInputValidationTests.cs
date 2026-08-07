using FluentAssertions;

namespace CryptoPal.Core.UnitTests;

public class ClientInputValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ReturnError_When_ValueIsMissing(string? value)
    {
        ClientInputValidation.ValidateNonEmpty("assetPlatformId", value)
            .Should().Be("assetPlatformId must contain at least one value.");
    }

    [Fact]
    public void Should_ReturnError_When_ValuesAreNull()
    {
        ClientInputValidation.ValidateNonEmptyValues("coins", null)
            .Should().Be("coins must contain at least one value.");
    }

    [Fact]
    public void Should_ReturnError_When_ValuesAreEmpty()
    {
        ClientInputValidation.ValidateNonEmptyValues("coins", [])
            .Should().Be("coins must contain at least one value.");
        ClientInputValidation.ValidateNonEmptyValues("coins", [""])
            .Should().Be("coins must contain at least one value.");
        ClientInputValidation.ValidateNonEmptyValues("coins", [" ", "  "])
            .Should().Be("coins must contain at least one value.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_ReturnError_When_DaysAreNotPositive(int days)
    {
        ClientInputValidation.ValidatePositiveDays(days)
            .Should().Be("days must be greater than zero.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("2025-12-30")]
    [InlineData("31-02-2025")]
    [InlineData("30/12/2025")]
    public void Should_ReturnError_When_DeveloperDateIsInvalid(string? date)
    {
        ClientInputValidation.ValidateDeveloperDate(date)
            .Should().Be("date must be a valid calendar date in dd-MM-yyyy format.");
    }

    [Fact]
    public void Should_ReturnNull_When_DeveloperDateIsValid()
    {
        ClientInputValidation.ValidateDeveloperDate("30-12-2025").Should().BeNull();
    }
}
