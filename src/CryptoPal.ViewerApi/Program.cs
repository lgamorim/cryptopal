using CryptoPal.ApiClient.CoinGecko;
using CryptoPal.Core;
using CryptoPal.ViewerApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

var apiKey = builder.Configuration["CoinGecko:ApiKey"]
    ?? throw new InvalidOperationException(
        "CoinGecko API key is not configured. Set it with: dotnet user-secrets set \"CoinGecko:ApiKey\" \"<your-key>\" --project src/CryptoPal.ViewerApi");

builder.Services.AddProblemDetails();
builder.Services.AddTransient<ICryptocurrencyService, CryptocurrencyService>();
builder.Services.AddHttpClient<ICoinGeckoClient, CoinGeckoClient>(client =>
    CoinGeckoClient.ConfigureHttpClient(client, apiKey));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapViewerApiEndpoints();

app.Run();

/// <summary>Marker type for WebApplication factory discovery in tests.</summary>
public partial class Program;
