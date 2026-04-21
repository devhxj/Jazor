using Jolt.VirtualDocuments.Models;

namespace Jolt.VirtualDocuments.Registry;

public sealed class InMemoryVirtualDocumentRegistry : IVirtualDocumentRegistry
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, VirtualDocument> _byProjectedPath =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string[]> _projectedPathsBySource =
        new(StringComparer.OrdinalIgnoreCase);

    public ValueTask<IReadOnlyList<VirtualDocument>> GetBySourceDocumentAsync(
        string sourceDocumentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceDocumentPath);
        cancellationToken.ThrowIfCancellationRequested();

        string[]? projectedPaths;
        lock (_gate)
        {
            if (!_projectedPathsBySource.TryGetValue(NormalizePath(sourceDocumentPath), out projectedPaths))
            {
                return ValueTask.FromResult<IReadOnlyList<VirtualDocument>>(Array.Empty<VirtualDocument>());
            }
        }

        var documents = new List<VirtualDocument>(projectedPaths.Length);
        lock (_gate)
        {
            foreach (var projectedPath in projectedPaths)
            {
                if (_byProjectedPath.TryGetValue(projectedPath, out var document))
                {
                    documents.Add(document);
                }
            }
        }

        return ValueTask.FromResult<IReadOnlyList<VirtualDocument>>(documents);
    }

    public ValueTask<VirtualDocument?> GetByProjectedDocumentAsync(
        string projectedDocumentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projectedDocumentPath);
        cancellationToken.ThrowIfCancellationRequested();

        VirtualDocument? document;
        lock (_gate)
        {
            _byProjectedPath.TryGetValue(NormalizePath(projectedDocumentPath), out document);
        }

        return ValueTask.FromResult(document);
    }

    public ValueTask UpsertAsync(
        IReadOnlyList<VirtualDocument> virtualDocuments,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(virtualDocuments);
        cancellationToken.ThrowIfCancellationRequested();

        if (virtualDocuments.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        var sourceDocumentPath = NormalizePath(virtualDocuments[0].Identity.SourceDocumentPath);
        var projectedPaths = new string[virtualDocuments.Count];
        for (var index = 0; index < virtualDocuments.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var virtualDocument = virtualDocuments[index];
            var currentSourceDocumentPath = NormalizePath(virtualDocument.Identity.SourceDocumentPath);
            if (!string.Equals(sourceDocumentPath, currentSourceDocumentPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("All virtual documents in a single upsert must share the same source document path.", nameof(virtualDocuments));
            }

            projectedPaths[index] = NormalizePath(virtualDocument.Identity.ProjectedDocumentPath);
        }

        lock (_gate)
        {
            if (_projectedPathsBySource.TryGetValue(sourceDocumentPath, out var previousProjectedPaths))
            {
                var currentProjectedPathSet = projectedPaths.ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var previousProjectedPath in previousProjectedPaths)
                {
                    if (!currentProjectedPathSet.Contains(previousProjectedPath))
                    {
                        _byProjectedPath.Remove(previousProjectedPath);
                    }
                }
            }

            for (var index = 0; index < virtualDocuments.Count; index++)
            {
                _byProjectedPath[projectedPaths[index]] = virtualDocuments[index];
            }

            _projectedPathsBySource[sourceDocumentPath] = projectedPaths;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveBySourceDocumentAsync(
        string sourceDocumentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceDocumentPath);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            if (_projectedPathsBySource.Remove(NormalizePath(sourceDocumentPath), out var projectedPaths))
            {
                foreach (var projectedPath in projectedPaths)
                {
                    _byProjectedPath.Remove(projectedPath);
                }
            }
        }

        return ValueTask.CompletedTask;
    }

    private static string NormalizePath(string documentPath)
        => documentPath.Replace('\\', '/');
}
