using CryptoPal.Core;
using CryptoPal.ApiClient.CoinGecko;
using CryptoPal.ViewerApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

await using var serviceProvider = ConfigureServices();
var runner = new ViewerAppRunner(
    serviceProvider.GetRequiredService<ICryptocurrencyService>(),
    Console.Out,
    Console.Error);
return await runner.RunAsync(args);

static ServiceProvider ConfigureServices()
{
    var configuration = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

    var apiKey = configuration["CoinGecko:ApiKey"]
        ?? throw new InvalidOperationException(
            "CoinGecko API key is not configured. Set it with: dotnet user-secrets set \"CoinGecko:ApiKey\" \"<your-key>\" --project src/CryptoPal.ViewerApp");

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging(builder => builder.AddConsole());
    serviceCollection.AddTransient<ICryptocurrencyService, CryptocurrencyService>();
    serviceCollection.AddHttpClient<ICoinGeckoClient, CoinGeckoClient>(client =>
        CoinGeckoClient.ConfigureHttpClient(client, apiKey));

    return serviceCollection.BuildServiceProvider();
}

partial class Program;
