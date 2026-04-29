using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Workspace;

public interface IJoltWorkspaceStore
{
    ValueTask<DocumentSnapshot?> GetDocumentAsync(
        string documentPath,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<DocumentSnapshot>> GetDocumentsAsync(
        IReadOnlyList<string> documentPaths,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(
        CancellationToken cancellationToken);

    ValueTask UpsertDocumentAsync(
        DocumentSnapshot documentSnapshot,
        CancellationToken cancellationToken);

    ValueTask RemoveDocumentAsync(
        string documentPath,
        CancellationToken cancellationToken);
}
