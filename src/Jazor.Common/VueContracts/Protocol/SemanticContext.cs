namespace ECMAScript.Contract.VueContracts.Protocol;

public sealed class SemanticContext(
	string contextKind,
	IReadOnlyList<DocumentSnapshot> relatedDocuments,
	IReadOnlyDictionary<string, string> properties)
{
	public string ContextKind { get; } = contextKind ?? throw new ArgumentNullException(nameof(contextKind));

	public IReadOnlyList<DocumentSnapshot> RelatedDocuments { get; } = relatedDocuments ?? throw new ArgumentNullException(nameof(relatedDocuments));

	public IReadOnlyDictionary<string, string> Properties { get; } = properties ?? throw new ArgumentNullException(nameof(properties));
}
