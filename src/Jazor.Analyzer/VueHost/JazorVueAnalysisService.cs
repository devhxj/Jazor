using Jazor.RazorVue.Protocol;

namespace Jazor.Vue;

public sealed class JazorVueAnalysisService : IVueAnalysisClient, IVueAnalysisRpcService
{
    private readonly InProcJazorVueAnalysisRuntime _runtime = new();

    public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
        => _runtime.AnalyzeJazorAsync(request, cancellationToken);
}

