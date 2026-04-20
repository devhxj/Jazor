namespace Jazor.VueContracts.Protocol;

public sealed class SemanticContext
{
    public SemanticContext(
        string contextKind,
        IReadOnlyList<DocumentSnapshot> relatedDocuments,
        IReadOnlyDictionary<string, string> properties)
    {
        ContextKind = contextKind ?? throw new ArgumentNullException(nameof(contextKind));
        RelatedDocuments = relatedDocuments ?? throw new ArgumentNullException(nameof(relatedDocuments));
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
    }

    public string ContextKind { get; }

    public IReadOnlyList<DocumentSnapshot> RelatedDocuments { get; }

    public IReadOnlyDictionary<string, string> Properties { get; }
}
