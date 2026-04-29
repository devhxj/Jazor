using ECMAScript.Contract.VueContracts.Protocol;

namespace Jazor.Vue;

public interface IVueAnalysisClient
{
    ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
}

