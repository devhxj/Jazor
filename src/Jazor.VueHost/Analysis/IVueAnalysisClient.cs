using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Analysis;

public interface IVueAnalysisClient
{
    ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
}
