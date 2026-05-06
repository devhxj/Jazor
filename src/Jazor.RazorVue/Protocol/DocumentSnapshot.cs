namespace Jazor.RazorVue.Protocol;

public enum DocumentKind
{
    Jazor,
    CSharp,
    Vue,
    JavaScript,
    TypeScript,
    Css,
    Unknown
}

public sealed class DocumentSnapshot(
	string documentPath,
	DocumentKind documentKind,
	string text,
	string? version)
{
	public string DocumentPath { get; } = documentPath ?? throw new ArgumentNullException(nameof(documentPath));

	public DocumentKind DocumentKind { get; } = documentKind;

	public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));

	public string? Version { get; } = version;
}
