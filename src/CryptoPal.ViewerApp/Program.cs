using CryptoPal.Core;
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

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging(builder => builder.AddConsole());
    serviceCollection.AddCryptoPal(configuration, "src/CryptoPal.ViewerApp");

    return serviceCollection.BuildServiceProvider();
}

partial class Program;
