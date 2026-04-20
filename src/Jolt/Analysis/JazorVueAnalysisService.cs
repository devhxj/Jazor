using Jazor.VueContracts.Protocol;

namespace Jolt.Analysis;

public sealed class JazorVueAnalysisService : IVueAnalysisClient, IVueAnalysisRpcService
{
    private readonly FallbackJazorAnalysisService _fallback = new();

    public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
        => _fallback.AnalyzeJazorAsync(request, cancellationToken);
}
