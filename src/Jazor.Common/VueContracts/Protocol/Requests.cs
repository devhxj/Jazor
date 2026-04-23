using System.Text.Json.Serialization;

namespace Jazor.Common.VueContracts.Protocol;

public sealed class AnalyzeJazorRequest(
	DocumentSnapshot jazorDocument,
	IReadOnlyList<DocumentSnapshot> relatedDocuments,
	SemanticContext? volarContext)
{
	[JsonPropertyName("jazorDocument")]
	public DocumentSnapshot JazorDocument { get; } = jazorDocument ?? throw new ArgumentNullException(nameof(jazorDocument));

	[JsonPropertyName("relatedDocuments")]
	public IReadOnlyList<DocumentSnapshot> RelatedDocuments { get; } = relatedDocuments ?? throw new ArgumentNullException(nameof(relatedDocuments));

	[JsonPropertyName("volarContext")]
	public SemanticContext? VolarContext { get; } = volarContext;
}

public sealed class AnalyzeJazorResponse(
	IReadOnlyList<DiagnosticRecord> diagnostics,
	IReadOnlyList<ImportDescriptor> imports,
	IReadOnlyList<ArtifactRecord> artifacts,
	IReadOnlyList<SourceMapDescriptor> sourceMaps)
{
	[JsonPropertyName("diagnostics")]
	public IReadOnlyList<DiagnosticRecord> Diagnostics { get; } = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

	[JsonPropertyName("imports")]
	public IReadOnlyList<ImportDescriptor> Imports { get; } = imports ?? throw new ArgumentNullException(nameof(imports));

	[JsonPropertyName("artifacts")]
	public IReadOnlyList<ArtifactRecord> Artifacts { get; } = artifacts ?? throw new ArgumentNullException(nameof(artifacts));

	[JsonPropertyName("sourceMaps")]
	public IReadOnlyList<SourceMapDescriptor> SourceMaps { get; } = sourceMaps ?? throw new ArgumentNullException(nameof(sourceMaps));
}

public sealed class GetVolarContextRequest(
	string documentPath,
	IReadOnlyList<string> relatedDocumentPaths)
{
	[JsonPropertyName("documentPath")]
	public string DocumentPath { get; } = documentPath ?? throw new ArgumentNullException(nameof(documentPath));

	[JsonPropertyName("relatedDocumentPaths")]
	public IReadOnlyList<string> RelatedDocumentPaths { get; } = relatedDocumentPaths ?? throw new ArgumentNullException(nameof(relatedDocumentPaths));
}

public sealed class GetVolarContextResponse(
	SemanticContext semanticContext,
	IReadOnlyList<ArtifactRecord> artifacts)
{
	[JsonPropertyName("semanticContext")]
	public SemanticContext SemanticContext { get; } = semanticContext ?? throw new ArgumentNullException(nameof(semanticContext));

	[JsonPropertyName("artifacts")]
	public IReadOnlyList<ArtifactRecord> Artifacts { get; } = artifacts ?? throw new ArgumentNullException(nameof(artifacts));
}

public sealed class GetVirtualArtifactRequest(
	string documentPath,
	string artifactKind,
	string? text,
	string? version)
{
	[JsonPropertyName("documentPath")]
	public string DocumentPath { get; } = documentPath ?? throw new ArgumentNullException(nameof(documentPath));

	[JsonPropertyName("artifactKind")]
	public string ArtifactKind { get; } = artifactKind ?? throw new ArgumentNullException(nameof(artifactKind));

	[JsonPropertyName("text")]
	public string? Text { get; } = text;

	[JsonPropertyName("version")]
	public string? Version { get; } = version;
}

public sealed class GetVirtualArtifactResponse(
	ArtifactRecord artifact,
	IReadOnlyList<DiagnosticRecord> diagnostics,
	IReadOnlyList<SourceMapDescriptor> sourceMaps)
{
	[JsonPropertyName("artifact")]
	public ArtifactRecord Artifact { get; } = artifact ?? throw new ArgumentNullException(nameof(artifact));

	[JsonPropertyName("diagnostics")]
	public IReadOnlyList<DiagnosticRecord> Diagnostics { get; } = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

	[JsonPropertyName("sourceMaps")]
	public IReadOnlyList<SourceMapDescriptor> SourceMaps { get; } = sourceMaps ?? throw new ArgumentNullException(nameof(sourceMaps));
}

public sealed class GetHotUpdatePlanRequest(
	string documentPath,
	DocumentKind documentKind,
	string? version)
{
	[JsonPropertyName("documentPath")]
	public string DocumentPath { get; } = documentPath ?? throw new ArgumentNullException(nameof(documentPath));

	[JsonPropertyName("documentKind")]
	public DocumentKind DocumentKind { get; } = documentKind;

	[JsonPropertyName("version")]
	public string? Version { get; } = version;
}

public sealed class GetHotUpdatePlanResponse(
	bool requiresFullReload,
	IReadOnlyList<string> affectedDocumentPaths,
	string reason)
{
	[JsonPropertyName("requiresFullReload")]
	public bool RequiresFullReload { get; } = requiresFullReload;

	[JsonPropertyName("affectedDocumentPaths")]
	public IReadOnlyList<string> AffectedDocumentPaths { get; } = affectedDocumentPaths ?? throw new ArgumentNullException(nameof(affectedDocumentPaths));

	[JsonPropertyName("reason")]
	public string Reason { get; } = reason ?? throw new ArgumentNullException(nameof(reason));
}
