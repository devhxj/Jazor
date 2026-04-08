using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;

namespace Jazor.VueHost.Services;

public sealed class NullVueAnalysisClient : IVueAnalysisClient
{
    public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.FromResult(new AnalyzeJazorResponse(
            diagnostics: Array.Empty<DiagnosticRecord>(),
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>()));
    }
}
