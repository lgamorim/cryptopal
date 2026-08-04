using CryptoPal.Core;
using CryptoPal.ViewerApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddProblemDetails();
builder.Services.AddCryptoPal(builder.Configuration, "src/CryptoPal.ViewerApi");

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapViewerApiEndpoints();

app.Run();

/// <summary>Marker type for WebApplication factory discovery in tests.</summary>
public partial class Program;
