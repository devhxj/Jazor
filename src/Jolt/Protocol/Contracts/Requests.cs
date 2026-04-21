using System.Text.Json.Serialization;

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

    [JsonPropertyName("jazorDocument")]
    public DocumentSnapshot JazorDocument { get; }

    [JsonPropertyName("relatedDocuments")]
    public IReadOnlyList<DocumentSnapshot> RelatedDocuments { get; }

    [JsonPropertyName("frontendContext")]
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

    [JsonPropertyName("diagnostics")]
    public IReadOnlyList<DiagnosticRecord> Diagnostics { get; }

    [JsonPropertyName("imports")]
    public IReadOnlyList<ImportDescriptor> Imports { get; }

    [JsonPropertyName("artifacts")]
    public IReadOnlyList<ArtifactRecord> Artifacts { get; }

    [JsonPropertyName("sourceMaps")]
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

    [JsonPropertyName("documentPath")]
    public string DocumentPath { get; }

    [JsonPropertyName("relatedDocumentPaths")]
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

    [JsonPropertyName("semanticContext")]
    public SemanticContext SemanticContext { get; }

    [JsonPropertyName("artifacts")]
    public IReadOnlyList<ArtifactRecord> Artifacts { get; }
}

public sealed class GetVirtualArtifactRequest
{
    public GetVirtualArtifactRequest(
        string documentPath,
        string artifactKind,
        string? text,
        string? version)
    {
        DocumentPath = documentPath ?? throw new ArgumentNullException(nameof(documentPath));
        ArtifactKind = artifactKind ?? throw new ArgumentNullException(nameof(artifactKind));
        Text = text;
        Version = version;
    }

    [JsonPropertyName("documentPath")]
    public string DocumentPath { get; }

    [JsonPropertyName("artifactKind")]
    public string ArtifactKind { get; }

    [JsonPropertyName("text")]
    public string? Text { get; }

    [JsonPropertyName("version")]
    public string? Version { get; }
}

public sealed class GetVirtualArtifactResponse
{
    public GetVirtualArtifactResponse(
        ArtifactRecord artifact,
        IReadOnlyList<DiagnosticRecord> diagnostics,
        IReadOnlyList<SourceMapDescriptor> sourceMaps)
    {
        Artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        SourceMaps = sourceMaps ?? throw new ArgumentNullException(nameof(sourceMaps));
    }

    [JsonPropertyName("artifact")]
    public ArtifactRecord Artifact { get; }

    [JsonPropertyName("diagnostics")]
    public IReadOnlyList<DiagnosticRecord> Diagnostics { get; }

    [JsonPropertyName("sourceMaps")]
    public IReadOnlyList<SourceMapDescriptor> SourceMaps { get; }
}

public sealed class GetHotUpdatePlanRequest
{
    public GetHotUpdatePlanRequest(
        string documentPath,
        DocumentKind documentKind,
        string? version)
    {
        DocumentPath = documentPath ?? throw new ArgumentNullException(nameof(documentPath));
        DocumentKind = documentKind;
        Version = version;
    }

    [JsonPropertyName("documentPath")]
    public string DocumentPath { get; }

    [JsonPropertyName("documentKind")]
    public DocumentKind DocumentKind { get; }

    [JsonPropertyName("version")]
    public string? Version { get; }
}

public sealed class GetHotUpdatePlanResponse
{
    public GetHotUpdatePlanResponse(
        bool requiresFullReload,
        IReadOnlyList<string> affectedDocumentPaths,
        string reason)
    {
        RequiresFullReload = requiresFullReload;
        AffectedDocumentPaths = affectedDocumentPaths ?? throw new ArgumentNullException(nameof(affectedDocumentPaths));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    [JsonPropertyName("requiresFullReload")]
    public bool RequiresFullReload { get; }

    [JsonPropertyName("affectedDocumentPaths")]
    public IReadOnlyList<string> AffectedDocumentPaths { get; }

    [JsonPropertyName("reason")]
    public string Reason { get; }
}
