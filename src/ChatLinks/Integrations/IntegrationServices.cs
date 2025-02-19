using System.Net.Http;

using GuildWars2;

using Microsoft.Extensions.DependencyInjection;

using Polly;

using SL.ChatLinks.StaticFiles;
using SL.ChatLinks.Storage;

namespace SL.ChatLinks.Integrations;

public static class IntegrationServices
{
    public static void AddDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions> configureOptions
    )
    {
        _ = services.Configure(configureOptions);

        _ = services.AddSingleton<IDbContextFactory, SqliteDbContextFactory>();
    }

    public static void AddGw2Client(this IServiceCollection services)
    {
        IHttpClientBuilder httpClientBuilder = services.AddHttpClient<Gw2Client>(
            static httpClient =>
            {
                httpClient.BaseAddress = BaseAddress.DefaultUri;
                httpClient.Timeout = Timeout.InfiniteTimeSpan;
            });

        _ = httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            MaxConnectionsPerServer = int.MaxValue
        });

        _ = httpClientBuilder.AddHttpMessageHandler(() =>
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

    public static void AddStaticDataClient(this IServiceCollection services)
    {
        IHttpClientBuilder httpClientBuilder = services.AddHttpClient<StaticDataClient>(
            static httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://bhm.blishhud.com/sliekens.chat_links/");
                httpClient.Timeout = Timeout.InfiniteTimeSpan;
            });
        _ = httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            MaxConnectionsPerServer = int.MaxValue
        });
        _ = httpClientBuilder.AddHttpMessageHandler(() =>
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
