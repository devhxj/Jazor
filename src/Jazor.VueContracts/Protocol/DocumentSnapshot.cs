namespace Jazor.VueContracts.Protocol;

public enum DocumentKind
{
    Jazor,
    Vue,
    JavaScript,
    TypeScript,
    Unknown
}

public sealed class DocumentSnapshot
{
    public DocumentSnapshot(
        string documentPath,
        DocumentKind documentKind,
        string text,
        string? version)
    {
        DocumentPath = documentPath ?? throw new ArgumentNullException(nameof(documentPath));
        DocumentKind = documentKind;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Version = version;
    }

    public string DocumentPath { get; }

    public DocumentKind DocumentKind { get; }

    public string Text { get; }

    public string? Version { get; }
}
