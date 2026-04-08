using Jazor.VueHost.VirtualDocuments.Mapping;

namespace Jazor.VueHost.VirtualDocuments.Models;

public sealed class VirtualDocument
{
    public VirtualDocument(
        VirtualDocumentIdentity identity,
        string text,
        ProjectionMap projectionMap,
        string? version)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ProjectionMap = projectionMap ?? throw new ArgumentNullException(nameof(projectionMap));
        Version = version;
    }

    public VirtualDocumentIdentity Identity { get; }

    public string Text { get; }

    public ProjectionMap ProjectionMap { get; }

    public string? Version { get; }
}
