namespace Jazor.VueContracts.Protocol;

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

public sealed class ImportDescriptor
{
    public ImportDescriptor(
        string localName,
        string source,
        ImportKind importKind,
        ImportBindingKind bindingKind,
        string? importedName,
        bool templateVisible)
    {
        LocalName = localName ?? throw new ArgumentNullException(nameof(localName));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        ImportKind = importKind;
        BindingKind = bindingKind;
        ImportedName = importedName;
        TemplateVisible = templateVisible;
    }

    public string LocalName { get; }

    public string Source { get; }

    public ImportKind ImportKind { get; }

    public ImportBindingKind BindingKind { get; }

    public string? ImportedName { get; }

    public bool TemplateVisible { get; }
}

public sealed class SourceMapDescriptor
{
    public SourceMapDescriptor(
        string sourcePath,
        string generatedPath,
        int sourceStart,
        int sourceLength,
        int generatedStart,
        int generatedLength)
    {
        SourcePath = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));
        GeneratedPath = generatedPath ?? throw new ArgumentNullException(nameof(generatedPath));
        SourceStart = sourceStart;
        SourceLength = sourceLength;
        GeneratedStart = generatedStart;
        GeneratedLength = generatedLength;
    }

    public string SourcePath { get; }

    public string GeneratedPath { get; }

    public int SourceStart { get; }

    public int SourceLength { get; }

    public int GeneratedStart { get; }

    public int GeneratedLength { get; }
}

public sealed class DiagnosticRecord
{
    public DiagnosticRecord(
        string id,
        DiagnosticSeverityKind severity,
        string message,
        string documentPath,
        int start,
        int length)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Severity = severity;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        DocumentPath = documentPath ?? throw new ArgumentNullException(nameof(documentPath));
        Start = start;
        Length = length;
    }

    public string Id { get; }

    public DiagnosticSeverityKind Severity { get; }

    public string Message { get; }

    public string DocumentPath { get; }

    public int Start { get; }

    public int Length { get; }
}

public sealed class ArtifactRecord
{
    public ArtifactRecord(
        string artifactName,
        string artifactKind,
        string content,
        string? contentHash)
    {
        ArtifactName = artifactName ?? throw new ArgumentNullException(nameof(artifactName));
        ArtifactKind = artifactKind ?? throw new ArgumentNullException(nameof(artifactKind));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        ContentHash = contentHash;
    }

    public string ArtifactName { get; }

    public string ArtifactKind { get; }

    public string Content { get; }

    public string? ContentHash { get; }
}
