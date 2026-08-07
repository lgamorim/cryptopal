using CryptoPal.ApiClient.CoinGecko;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoPal.Core;

/// <summary>Registers CryptoPal core services and the CoinGecko API client.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ICryptocurrencyService"/> and <see cref="ICoinGeckoClient"/> to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration containing <c>CoinGecko:ApiKey</c>.</param>
    /// <param name="userSecretsProjectPath">Project path shown in the error message when the API key is missing.</param>
    public static IServiceCollection AddCryptoPal(
        this IServiceCollection services,
        IConfiguration configuration,
        string userSecretsProjectPath)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(userSecretsProjectPath);

        var apiKey = configuration["CoinGecko:ApiKey"]
            ?? throw new InvalidOperationException(
                $"CoinGecko API key is not configured. Set it with: dotnet user-secrets set \"CoinGecko:ApiKey\" \"<your-key>\" --project {userSecretsProjectPath}");

        var cacheSeconds = configuration.GetValue("CoinGecko:CacheSeconds", 60);

        services.AddMemoryCache();
        services.AddTransient<CryptocurrencyService>();
        services.AddTransient<ICryptocurrencyService>(serviceProvider =>
            new CachingCryptocurrencyService(
                serviceProvider.GetRequiredService<CryptocurrencyService>(),
                serviceProvider.GetRequiredService<IMemoryCache>(),
                TimeSpan.FromSeconds(cacheSeconds)));
        services.AddHttpClient<ICoinGeckoClient, CoinGeckoClient>()
            .AddCoinGeckoHttpClient(apiKey);

        return services;
    }
}
