using System.Collections.Concurrent;
using ECMAScript.Contract.VueContracts.Protocol;

namespace Jolt.Workspace;

public sealed class InMemoryWorkspaceStore : IJoltWorkspaceStore
{
    private readonly ConcurrentDictionary<string, DocumentSnapshot> _documents =
        new(WorkspacePathComparison.StringComparer);

    public ValueTask<DocumentSnapshot?> GetDocumentAsync(
        string documentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documentPath);
        cancellationToken.ThrowIfCancellationRequested();

        _documents.TryGetValue(NormalizeDocumentPath(documentPath), out var snapshot);
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
            if (_documents.TryGetValue(NormalizeDocumentPath(documentPath), out var snapshot))
                documents.Add(snapshot);
        }

        return ValueTask.FromResult<IReadOnlyList<DocumentSnapshot>>(documents);
    }

    public ValueTask<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<DocumentSnapshot> documents = _documents.Values
            .OrderBy(static document => document.DocumentPath, WorkspacePathComparison.StringComparer)
            .ToArray();

        return ValueTask.FromResult(documents);
    }

    public ValueTask UpsertDocumentAsync(
        DocumentSnapshot documentSnapshot,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documentSnapshot);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedPath = NormalizeDocumentPath(documentSnapshot.DocumentPath);
        var normalizedSnapshot = string.Equals(normalizedPath, documentSnapshot.DocumentPath, StringComparison.Ordinal)
            ? documentSnapshot
            : new DocumentSnapshot(
                normalizedPath,
                documentSnapshot.DocumentKind,
                documentSnapshot.Text,
                documentSnapshot.Version);
        _documents[normalizedPath] = normalizedSnapshot;
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveDocumentAsync(
        string documentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documentPath);
        cancellationToken.ThrowIfCancellationRequested();

        _documents.TryRemove(NormalizeDocumentPath(documentPath), out _);
        return ValueTask.CompletedTask;
    }

    private static string NormalizeDocumentPath(string documentPath)
    {
        return Path.IsPathRooted(documentPath)
            ? Path.GetFullPath(documentPath).Replace('\\', '/')
            : documentPath.Replace('\\', '/');
    }
}
