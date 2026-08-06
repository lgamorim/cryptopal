using System.Net;
using CryptoPal.ApiClient.CoinGecko;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace CryptoPal.Core;

internal static class CoinGeckoHttpClientBuilderExtensions
{
    private const int MaxRetryAttempts = 3;

    internal static IHttpClientBuilder AddCoinGeckoHttpClient(this IHttpClientBuilder builder, string apiKey)
    {
        builder.ConfigureHttpClient(client => CoinGeckoClient.ConfigureHttpClient(client, apiKey));
        builder.AddResilienceHandler("coin-gecko-rate-limit", ConfigureRateLimitRetry);
        return builder;
    }

    private static void ConfigureRateLimitRetry(ResiliencePipelineBuilder<HttpResponseMessage> builder) =>
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = MaxRetryAttempts,
            BackoffType = DelayBackoffType.Constant,
            Delay = TimeSpan.FromSeconds(1),
            ShouldHandle = static args => ValueTask.FromResult(
                args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests),
            DelayGenerator = static args =>
            {
                if (args.Outcome.Result?.Headers.RetryAfter is { } retryAfter)
                {
                    if (retryAfter.Delta is { } delta)
                    {
                        return ValueTask.FromResult<TimeSpan?>(delta);
                    }

                    if (retryAfter.Date is { } date)
                    {
                        var delay = date - DateTimeOffset.UtcNow;
                        return ValueTask.FromResult<TimeSpan?>(delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
                    }
                }

                return ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(1));
            }
        });
}
