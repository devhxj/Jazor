namespace Jazor.VueContracts.Protocol;

public sealed class AnalyzeJazorRequest
{
    public AnalyzeJazorRequest(
        DocumentSnapshot jazorDocument,
        IReadOnlyList<DocumentSnapshot> relatedDocuments,
        SemanticContext? frontendContext)
    {
        JazorDocument = jazorDocument ?? throw new ArgumentNullException(nameof(jazorDocument));
        RelatedDocuments = relatedDocuments ?? throw new ArgumentNullException(nameof(relatedDocuments));
        FrontendContext = frontendContext;
    }

    public DocumentSnapshot JazorDocument { get; }

    public IReadOnlyList<DocumentSnapshot> RelatedDocuments { get; }

    public SemanticContext? FrontendContext { get; }
}

public sealed class AnalyzeJazorResponse
{
    public AnalyzeJazorResponse(
        IReadOnlyList<DiagnosticRecord> diagnostics,
        IReadOnlyList<ImportDescriptor> imports,
        IReadOnlyList<ArtifactRecord> artifacts,
        IReadOnlyList<SourceMapDescriptor> sourceMaps)
    {
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        Imports = imports ?? throw new ArgumentNullException(nameof(imports));
        Artifacts = artifacts ?? throw new ArgumentNullException(nameof(artifacts));
        SourceMaps = sourceMaps ?? throw new ArgumentNullException(nameof(sourceMaps));
    }

    public IReadOnlyList<DiagnosticRecord> Diagnostics { get; }

    public IReadOnlyList<ImportDescriptor> Imports { get; }

    public IReadOnlyList<ArtifactRecord> Artifacts { get; }

    public IReadOnlyList<SourceMapDescriptor> SourceMaps { get; }
}

public sealed class GetFrontendContextRequest
{
    public GetFrontendContextRequest(
        string documentPath,
        IReadOnlyList<string> relatedDocumentPaths)
    {
        DocumentPath = documentPath ?? throw new ArgumentNullException(nameof(documentPath));
        RelatedDocumentPaths = relatedDocumentPaths ?? throw new ArgumentNullException(nameof(relatedDocumentPaths));
    }

    public string DocumentPath { get; }

    public IReadOnlyList<string> RelatedDocumentPaths { get; }
}

public sealed class GetFrontendContextResponse
{
    public GetFrontendContextResponse(
        SemanticContext semanticContext,
        IReadOnlyList<ArtifactRecord> artifacts)
    {
        SemanticContext = semanticContext ?? throw new ArgumentNullException(nameof(semanticContext));
        Artifacts = artifacts ?? throw new ArgumentNullException(nameof(artifacts));
    }

    public SemanticContext SemanticContext { get; }

    public IReadOnlyList<ArtifactRecord> Artifacts { get; }
}
