using System.Collections.Concurrent;
using Jolt.VirtualDocuments.Models;

namespace Jolt.VirtualDocuments.Registry;

public sealed class InMemoryVirtualDocumentRegistry : IVirtualDocumentRegistry
{
    private readonly ConcurrentDictionary<string, VirtualDocument> _byProjectedPath =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string[]> _projectedPathsBySource =
        new(StringComparer.OrdinalIgnoreCase);

    public ValueTask<IReadOnlyList<VirtualDocument>> GetBySourceDocumentAsync(
        string sourceDocumentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceDocumentPath);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_projectedPathsBySource.TryGetValue(NormalizePath(sourceDocumentPath), out var projectedPaths))
        {
            return ValueTask.FromResult<IReadOnlyList<VirtualDocument>>(Array.Empty<VirtualDocument>());
        }

        var documents = projectedPaths
            .Select(path => _byProjectedPath.TryGetValue(path, out var document) ? document : null)
            .Where(static document => document is not null)
            .Cast<VirtualDocument>()
            .ToArray();
        return ValueTask.FromResult<IReadOnlyList<VirtualDocument>>(documents);
    }

    public ValueTask<VirtualDocument?> GetByProjectedDocumentAsync(
        string projectedDocumentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projectedDocumentPath);
        cancellationToken.ThrowIfCancellationRequested();

        _byProjectedPath.TryGetValue(NormalizePath(projectedDocumentPath), out var document);
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
        var projectedPaths = new List<string>(virtualDocuments.Count);

        foreach (var virtualDocument in virtualDocuments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalizedProjectedPath = NormalizePath(virtualDocument.Identity.ProjectedDocumentPath);
            _byProjectedPath[normalizedProjectedPath] = virtualDocument;
            projectedPaths.Add(normalizedProjectedPath);
        }

        _projectedPathsBySource[sourceDocumentPath] = projectedPaths.ToArray();
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveBySourceDocumentAsync(
        string sourceDocumentPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceDocumentPath);
        cancellationToken.ThrowIfCancellationRequested();

        if (_projectedPathsBySource.TryRemove(NormalizePath(sourceDocumentPath), out var projectedPaths))
        {
            foreach (var projectedPath in projectedPaths)
            {
                _byProjectedPath.TryRemove(projectedPath, out _);
            }
        }

        return ValueTask.CompletedTask;
    }

    private static string NormalizePath(string documentPath)
        => documentPath.Replace('\\', '/');
}
