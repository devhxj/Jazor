using Jazor.VueHost.VirtualDocuments.Models;

namespace Jazor.VueHost.VirtualDocuments.Registry;

public interface IVirtualDocumentRegistry
{
    ValueTask<IReadOnlyList<VirtualDocument>> GetBySourceDocumentAsync(
        string sourceDocumentPath,
        CancellationToken cancellationToken);

    ValueTask<VirtualDocument?> GetByProjectedDocumentAsync(
        string projectedDocumentPath,
        CancellationToken cancellationToken);

    ValueTask UpsertAsync(
        IReadOnlyList<VirtualDocument> virtualDocuments,
        CancellationToken cancellationToken);

    ValueTask RemoveBySourceDocumentAsync(
        string sourceDocumentPath,
        CancellationToken cancellationToken);
}
