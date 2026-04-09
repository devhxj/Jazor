using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Analysis;

public sealed class JazorVueAnalysisService : IVueAnalysisRpcService
{
    private readonly FallbackJazorAnalysisService _fallback = new();

    public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
        => _fallback.AnalyzeJazorAsync(request, cancellationToken);
}
