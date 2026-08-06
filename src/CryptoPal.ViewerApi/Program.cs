using CryptoPal.Core;
using CryptoPal.ViewerApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddCryptoPal(builder.Configuration, "src/CryptoPal.ViewerApi");

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

app.MapOpenApi();
app.MapViewerApiEndpoints();

app.Run();

/// <summary>Marker type for WebApplication factory discovery in tests.</summary>
public partial class Program;
