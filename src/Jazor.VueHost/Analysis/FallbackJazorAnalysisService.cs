using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Hosting;

namespace Jazor.VueHost.Analysis;

internal sealed class FallbackJazorAnalysisService
{
    private readonly JazorVueParser _parser = new();
    private readonly JazorVueCompiler _compiler = new();

    public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        FallbackTelemetry.ReportActivation(
            component: "analysisService",
            mode: "inProcFallback",
            reason: "analysis-rpc-unavailable",
            documentPath: request.JazorDocument.DocumentPath);

        var document = _parser.Parse(
            request.JazorDocument.DocumentPath,
            request.JazorDocument.Text);
        var compilation = _compiler.Compile(document);

        var diagnostics = new List<DiagnosticRecord>(compilation.Diagnostics.Count + 4);
        diagnostics.AddRange(compilation.Diagnostics
            .Select((message, index) => new DiagnosticRecord(
                id: $"JAZORVUE{index + 1:000}",
                severity: DiagnosticSeverityKind.Warning,
                message: message,
                documentPath: request.JazorDocument.DocumentPath,
                start: 0,
                length: 0)));
        diagnostics.AddRange(LegacyImportDirectiveCatalog.FindOccurrences(request.JazorDocument.Text)
            .Select(occurrence => new DiagnosticRecord(
                id: LegacyImportDirectiveCatalog.DiagnosticCode,
                severity: DiagnosticSeverityKind.Error,
                message: LegacyImportDirectiveCatalog.CreateDiagnosticMessage(occurrence.Kind),
                documentPath: request.JazorDocument.DocumentPath,
                start: occurrence.Start,
                length: occurrence.Length)));

        var imports = document.Imports
            .SelectMany(import => import.Bindings.Select(binding => new ImportDescriptor(
                localName: binding.LocalName,
                source: import.Source,
                importKind: MapImportKind(import.Kind),
                bindingKind: MapBindingKind(binding.BindingKind),
                importedName: binding.ImportedName,
                templateVisible: import.Kind == JazorImportKind.VueImport)))
            .ToArray();

        var artifacts = new[]
        {
            new ArtifactRecord(
                artifactName: "virtual:" + request.JazorDocument.DocumentPath + ".vue",
                artifactKind: "vue-sfc",
                content: compilation.GeneratedVueText,
                contentHash: null),
            new ArtifactRecord(
                artifactName: "virtual:" + request.JazorDocument.DocumentPath + ".externals.g.cs",
                artifactKind: "csharp-externals",
                content: compilation.GeneratedExternalDeclarationsText,
                contentHash: null)
        };
        var sourceMaps = new[]
        {
            new SourceMapDescriptor(
                sourcePath: request.JazorDocument.DocumentPath,
                generatedPath: artifacts[0].ArtifactName,
                sourceStart: 0,
                sourceLength: request.JazorDocument.Text.Length,
                generatedStart: 0,
                generatedLength: artifacts[0].Content.Length),
            new SourceMapDescriptor(
                sourcePath: request.JazorDocument.DocumentPath,
                generatedPath: artifacts[1].ArtifactName,
                sourceStart: 0,
                sourceLength: request.JazorDocument.Text.Length,
                generatedStart: 0,
                generatedLength: artifacts[1].Content.Length)
        };

        return ValueTask.FromResult(new AnalyzeJazorResponse(
            diagnostics: diagnostics,
            imports: imports,
            artifacts: artifacts,
            sourceMaps: sourceMaps));
    }

    private static ImportKind MapImportKind(JazorImportKind importKind)
        => importKind == JazorImportKind.VueImport
            ? ImportKind.VueImport
            : ImportKind.JSImport;

    private static ImportBindingKind MapBindingKind(JazorImportBindingKind bindingKind)
        => bindingKind switch
        {
            JazorImportBindingKind.Default => ImportBindingKind.Default,
            JazorImportBindingKind.Namespace => ImportBindingKind.Namespace,
            _ => ImportBindingKind.Named
        };
}
