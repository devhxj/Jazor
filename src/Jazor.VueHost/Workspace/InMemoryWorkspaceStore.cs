using System.Collections.Concurrent;
using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Workspace;

public sealed class InMemoryWorkspaceStore : IVueHostWorkspaceStore
{
    private readonly ConcurrentDictionary<string, DocumentSnapshot> _documents =
        new(StringComparer.OrdinalIgnoreCase);

    public ValueTask<DocumentSnapshot?> GetDocumentAsync(
        string documentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documentPath);
        cancellationToken.ThrowIfCancellationRequested();

        _documents.TryGetValue(documentPath, out var snapshot);
        return ValueTask.FromResult(snapshot);
    }

    public ValueTask<IReadOnlyList<DocumentSnapshot>> GetDocumentsAsync(
        IReadOnlyList<string> documentPaths,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documentPaths);
        cancellationToken.ThrowIfCancellationRequested();

        var documents = new List<DocumentSnapshot>(documentPaths.Count);
        foreach (var documentPath in documentPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_documents.TryGetValue(documentPath, out var snapshot))
                documents.Add(snapshot);
        }

        return ValueTask.FromResult<IReadOnlyList<DocumentSnapshot>>(documents);
    }

    public ValueTask<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<DocumentSnapshot> documents = _documents.Values
            .OrderBy(static document => document.DocumentPath, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ValueTask.FromResult(documents);
    }

    public ValueTask UpsertDocumentAsync(
        DocumentSnapshot documentSnapshot,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documentSnapshot);
        cancellationToken.ThrowIfCancellationRequested();

        _documents[documentSnapshot.DocumentPath] = documentSnapshot;
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveDocumentAsync(
        string documentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documentPath);
        cancellationToken.ThrowIfCancellationRequested();

        _documents.TryRemove(documentPath, out _);
        return ValueTask.CompletedTask;
    }
}
