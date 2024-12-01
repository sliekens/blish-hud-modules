using System.Net.Http;

using Polly;

namespace ChatLinksModule.Integrations;

public class ResilienceHandler : DelegatingHandler
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public ResilienceHandler(ResiliencePipeline<HttpResponseMessage> pipeline)
    {
        _pipeline = pipeline;
    }

    public ResilienceHandler(ResiliencePipeline<HttpResponseMessage> pipeline,
        HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
        _pipeline = pipeline;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _pipeline
            .ExecuteAsync(async ct => await base.SendAsync(request, ct).ConfigureAwait(false), cancellationToken)
            .ConfigureAwait(false);
    }
}