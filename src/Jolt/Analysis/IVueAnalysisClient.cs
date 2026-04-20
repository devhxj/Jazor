using Jazor.VueContracts.Protocol;

namespace Jolt.Analysis;

public interface IVueAnalysisClient
{
    ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
}
