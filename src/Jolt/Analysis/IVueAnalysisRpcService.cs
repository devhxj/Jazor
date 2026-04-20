using Jazor.VueContracts.Protocol;

namespace Jolt.Analysis;

public interface IVueAnalysisRpcService
{
    ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
}
