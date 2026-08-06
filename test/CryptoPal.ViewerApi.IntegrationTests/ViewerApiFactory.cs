using CryptoPal.ApiClient.CoinGecko;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CryptoPal.ViewerApi.IntegrationTests;

public sealed class ViewerApiFactory : WebApplicationFactory<Program>
{
    public FakeCoinGeckoClient FakeCoinGeckoClient { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseSetting("CoinGecko:ApiKey", "integration-test-api-key");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICoinGeckoClient>();
            services.AddSingleton<ICoinGeckoClient>(FakeCoinGeckoClient);
        });
    }
}
