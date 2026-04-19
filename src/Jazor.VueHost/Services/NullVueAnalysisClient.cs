using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Hosting;

namespace Jazor.VueHost.Services;

public sealed class NullVueAnalysisClient : IVueAnalysisClient
{
    public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        FallbackTelemetry.ReportActivation(
            component: "analysisClient",
            mode: "null",
            reason: "analysis-transport-unavailable",
            documentPath: request.JazorDocument.DocumentPath);

        return ValueTask.FromResult(new AnalyzeJazorResponse(
            diagnostics: Array.Empty<DiagnosticRecord>(),
            imports: Array.Empty<ImportDescriptor>(),
            artifacts: Array.Empty<ArtifactRecord>(),
            sourceMaps: Array.Empty<SourceMapDescriptor>()));
    }
}
