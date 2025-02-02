using System.Net.Http;

using GuildWars2;

using Microsoft.Extensions.DependencyInjection;

using Polly;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.Integrations;

public static class Integrations
{
    public static void AddDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions> configureOptions
    )
    {
        services.Configure(configureOptions);

        services.AddSingleton<IDbContextFactory, SqliteDbContextFactory>();
    }

    public static void AddGw2Client(this IServiceCollection services)
    {
        IHttpClientBuilder httpClientBuilder = services.AddHttpClient<Gw2Client>(
            static httpClient =>
            {
                httpClient.Timeout = Timeout.InfiniteTimeSpan;
            });

        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            MaxConnectionsPerServer = int.MaxValue
        });

        httpClientBuilder.AddHttpMessageHandler(() =>
        {
            ResiliencePipeline<HttpResponseMessage> pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddTimeout(Resiliency.TotalTimeoutStrategy)
                .AddRetry(Resiliency.RetryStrategy)
                .AddCircuitBreaker(Resiliency.CircuitBreakerStrategy)
                .AddHedging(Resiliency.HedgingStrategy)
                .AddTimeout(Resiliency.AttemptTimeoutStrategy)
                .Build();
            return new ResilienceHandler(pipeline);
        });
    }
}
