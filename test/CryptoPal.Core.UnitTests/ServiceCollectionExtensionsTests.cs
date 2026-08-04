using CryptoPal.ApiClient.CoinGecko;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoPal.Core.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Should_RegisterCryptocurrencyService_When_AddCryptoPalCalled()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration("test-api-key");

        services.AddCryptoPal(configuration, "src/CryptoPal.ViewerApp");
        using var provider = services.BuildServiceProvider();

        provider.GetService<ICryptocurrencyService>().Should().NotBeNull();
    }

    [Fact]
    public void Should_RegisterCoinGeckoClient_When_AddCryptoPalCalled()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration("test-api-key");

        services.AddCryptoPal(configuration, "src/CryptoPal.ViewerApp");
        using var provider = services.BuildServiceProvider();

        provider.GetService<ICoinGeckoClient>().Should().NotBeNull();
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_ApiKeyMissing()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(apiKey: null);

        var act = () => services.AddCryptoPal(configuration, "src/CryptoPal.ViewerApp");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CoinGecko API key is not configured*src/CryptoPal.ViewerApp*");
    }

    private static IConfiguration CreateConfiguration(string? apiKey)
    {
        var settings = new Dictionary<string, string?>();
        if (apiKey is not null)
        {
            settings["CoinGecko:ApiKey"] = apiKey;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
