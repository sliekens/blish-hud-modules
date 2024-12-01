using System.Net.Http;

using GuildWars2;

using Microsoft.Extensions.DependencyInjection;

using Polly;

namespace ChatLinksModule.Integrations;

internal static class Integrations
{
    internal static void AddGw2Client(this IServiceCollection services)
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