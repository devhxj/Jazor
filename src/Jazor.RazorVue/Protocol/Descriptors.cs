namespace Jazor.RazorVue.Protocol;

public enum ImportKind
{
    JSImport,
    VueImport
}

public enum ImportBindingKind
{
    Default,
    Named,
    Namespace
}

public enum DiagnosticSeverityKind
{
    Info,
    Warning,
    Error
}

public sealed class ImportDescriptor(
	string localName,
	string source,
	ImportKind importKind,
	ImportBindingKind bindingKind,
	string? importedName,
	bool templateVisible)
{
	public string LocalName { get; } = localName ?? throw new ArgumentNullException(nameof(localName));

	public string Source { get; } = source ?? throw new ArgumentNullException(nameof(source));

	public ImportKind ImportKind { get; } = importKind;

	public ImportBindingKind BindingKind { get; } = bindingKind;

	public string? ImportedName { get; } = importedName;

	public bool TemplateVisible { get; } = templateVisible;
}

public sealed class SourceMapDescriptor(
	string sourcePath,
	string generatedPath,
	int sourceStart,
	int sourceLength,
	int generatedStart,
	int generatedLength)
{
	public string SourcePath { get; } = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));

	public string GeneratedPath { get; } = generatedPath ?? throw new ArgumentNullException(nameof(generatedPath));

	public int SourceStart { get; } = sourceStart;

	public int SourceLength { get; } = sourceLength;

	public int GeneratedStart { get; } = generatedStart;

	public int GeneratedLength { get; } = generatedLength;
}

public sealed class DiagnosticRecord(
	string id,
	DiagnosticSeverityKind severity,
	string message,
	string documentPath,
	int start,
	int length)
{
	public string Id { get; } = id ?? throw new ArgumentNullException(nameof(id));

	public DiagnosticSeverityKind Severity { get; } = severity;

	public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));

	public string DocumentPath { get; } = documentPath ?? throw new ArgumentNullException(nameof(documentPath));

	public int Start { get; } = start;

	public int Length { get; } = length;
}

public sealed class ArtifactRecord(
	string artifactName,
	string artifactKind,
	string content,
	string? contentHash)
{
	public string ArtifactName { get; } = artifactName ?? throw new ArgumentNullException(nameof(artifactName));

	public string ArtifactKind { get; } = artifactKind ?? throw new ArgumentNullException(nameof(artifactKind));

	public string Content { get; } = content ?? throw new ArgumentNullException(nameof(content));

	public string? ContentHash { get; } = contentHash;
}
