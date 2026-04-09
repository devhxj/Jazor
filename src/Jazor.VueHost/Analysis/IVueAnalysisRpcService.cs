using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Analysis;

public interface IVueAnalysisRpcService
{
    ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
}
