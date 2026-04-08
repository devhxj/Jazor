using Jazor.VueContracts.Protocol;

namespace Jazor.Vue.Analysis.Runtime;

public interface IVueAnalysisRpcService
{
    ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
}
