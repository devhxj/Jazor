using Jazor.Common.VueContracts.Protocol;

namespace Jolt.Analysis;

internal static class VueAnalysisClientExtensions
{
    public static async ValueTask<AnalyzeJazorResponse> AnalyzeWithFallbackAsync(
        this IVueAnalysisClient analysisClient,
        FallbackJazorAnalysisService fallbackAnalysisService,
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken,
        Func<AnalyzeJazorResponse, bool>? acceptResponse = null)
    {
        ArgumentNullException.ThrowIfNull(analysisClient);
        ArgumentNullException.ThrowIfNull(fallbackAnalysisService);
        ArgumentNullException.ThrowIfNull(request);

        acceptResponse ??= HasUsableOutput;

        var response = await analysisClient.AnalyzeJazorAsync(request, cancellationToken);
        if (acceptResponse(response))
        {
            return response;
        }

        return await fallbackAnalysisService.AnalyzeJazorAsync(request, cancellationToken);
    }

    public static bool HasUsableOutput(AnalyzeJazorResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.Diagnostics.Count > 0
            || response.Imports.Count > 0
            || response.Artifacts.Count > 0
            || response.SourceMaps.Count > 0;
    }

    public static ArtifactRecord? FindArtifact(
        this AnalyzeJazorResponse analysisResponse,
        string artifactKind)
    {
        ArgumentNullException.ThrowIfNull(analysisResponse);
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactKind);

        return analysisResponse.Artifacts.FirstOrDefault(candidate =>
            string.Equals(candidate.ArtifactKind, artifactKind, StringComparison.OrdinalIgnoreCase));
    }
}
