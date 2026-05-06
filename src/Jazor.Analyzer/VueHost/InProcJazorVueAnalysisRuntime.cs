using Jazor.RazorVue;
using Jazor.RazorVue.Protocol;

namespace Jazor.Vue;

internal sealed class InProcJazorVueAnalysisRuntime
{
    private readonly JazorVueCompiler _compiler = new();

    public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var document = JazorVueParser.Parse(
            request.JazorDocument.DocumentPath,
            request.JazorDocument.Text);
        var compilation = _compiler.Compile(document);

        var diagnostics = new List<DiagnosticRecord>(compilation.Diagnostics.Count + 4);
        diagnostics.AddRange(compilation.Diagnostics
            .Select((message, index) => new DiagnosticRecord(
                id: string.Format("JAZORVUE{0:000}", index + 1),
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
            .SelectMany(importDirective => importDirective.Bindings.Select(binding => new ImportDescriptor(
                localName: binding.LocalName,
                source: importDirective.Source,
                importKind: MapImportKind(importDirective.Kind),
                bindingKind: MapBindingKind(binding.BindingKind),
                importedName: binding.ImportedName,
                templateVisible: importDirective.Kind == JazorImportKind.VueImport)))
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
        var vueArtifact = artifacts.FirstOrDefault(static artifact => artifact.ArtifactKind == "vue-sfc");
        var externalsArtifact = artifacts.FirstOrDefault(static artifact => artifact.ArtifactKind == "csharp-externals");
        if (vueArtifact is null || externalsArtifact is null)
        {
            throw new InvalidOperationException("Fallback analysis did not produce the expected virtual artifacts.");
        }

        var sourceMaps = new[]
        {
            new SourceMapDescriptor(
                sourcePath: request.JazorDocument.DocumentPath,
                generatedPath: vueArtifact.ArtifactName,
                sourceStart: 0,
                sourceLength: request.JazorDocument.Text.Length,
                generatedStart: 0,
                generatedLength: vueArtifact.Content.Length),
            new SourceMapDescriptor(
                sourcePath: request.JazorDocument.DocumentPath,
                generatedPath: externalsArtifact.ArtifactName,
                sourceStart: 0,
                sourceLength: request.JazorDocument.Text.Length,
                generatedStart: 0,
                generatedLength: externalsArtifact.Content.Length)
        };

        return new ValueTask<AnalyzeJazorResponse>(new AnalyzeJazorResponse(
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

