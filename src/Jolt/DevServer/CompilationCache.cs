using System.Diagnostics.CodeAnalysis;

namespace Jolt.DevServer;

internal sealed class CompilationCache
{
    internal const int DefaultMaxEntries = 512;

    private readonly int _maxEntries;
    private readonly Dictionary<string, CacheEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _leastRecentlyUsedPaths = new();
    private readonly Lock _gate = new();

    public CompilationCache(int maxEntries = DefaultMaxEntries)
    {
        if (maxEntries <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxEntries));
        }

        _maxEntries = maxEntries;
    }

    public bool TryGet(string absolutePath, string contentHash, [NotNullWhen(true)] out CompilationResult? result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);
        var normalizedPath = NormalizePath(absolutePath);

        lock (_gate)
        {
            if (_entries.TryGetValue(normalizedPath, out var entry) &&
                string.Equals(entry.ContentHash, contentHash, StringComparison.Ordinal))
            {
                TouchCore(normalizedPath, entry);
                result = entry.Result;
                return true;
            }
        }

        result = null;
        return false;
    }

    public bool TryPeek(string absolutePath, [NotNullWhen(true)] out CompilationResult? result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var normalizedPath = NormalizePath(absolutePath);

        lock (_gate)
        {
            if (_entries.TryGetValue(normalizedPath, out var entry))
            {
                TouchCore(normalizedPath, entry);
                result = entry.Result;
                return true;
            }
        }

        result = null;
        return false;
    }

    public IReadOnlyList<string> Set(string absolutePath, string contentHash, CompilationResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);
        ArgumentNullException.ThrowIfNull(result);
        var normalizedPath = NormalizePath(absolutePath);

        lock (_gate)
        {
            if (_entries.Remove(normalizedPath, out var oldEntry))
            {
                _leastRecentlyUsedPaths.Remove(oldEntry.AccessNode);
            }

            var accessNode = new LinkedListNode<string>(normalizedPath);
            _leastRecentlyUsedPaths.AddFirst(accessNode);
            _entries[normalizedPath] = new CacheEntry(contentHash, result, accessNode);
            return EvictOverflowCore();
        }
    }

    public bool Invalidate(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var normalizedPath = NormalizePath(absolutePath);

        lock (_gate)
        {
            return RemoveCore(normalizedPath);
        }
    }

    public IReadOnlyList<string> InvalidateAll()
    {
        lock (_gate)
        {
            var paths = _entries.Keys.ToArray();
            _entries.Clear();
            _leastRecentlyUsedPaths.Clear();
            return paths;
        }
    }

    public IReadOnlyList<string> GetPaths()
    {
        lock (_gate)
        {
            return _entries.Keys.ToArray();
        }
    }

    public IReadOnlyList<KeyValuePair<string, CompilationResult>> GetEntries()
    {
        lock (_gate)
        {
            return _entries
                .Select(static entry => new KeyValuePair<string, CompilationResult>(entry.Key, entry.Value.Result))
                .ToArray();
        }
    }

    // Normalize once at the cache boundary so slash variants of the same absolute path reuse one entry.
    private static string NormalizePath(string absolutePath)
        => Path.GetFullPath(absolutePath).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    private void TouchCore(string normalizedPath, CacheEntry entry)
    {
        _leastRecentlyUsedPaths.Remove(entry.AccessNode);
        _leastRecentlyUsedPaths.AddFirst(entry.AccessNode);
    }

    private IReadOnlyList<string> EvictOverflowCore()
    {
        if (_entries.Count <= _maxEntries)
        {
            return [];
        }

        var evictedPaths = new List<string>();
        while (_entries.Count > _maxEntries && _leastRecentlyUsedPaths.Last is { } leastRecentlyUsed)
        {
            var evictedPath = leastRecentlyUsed.Value;
            if (RemoveCore(evictedPath))
            {
                evictedPaths.Add(evictedPath);
            }
        }

        return evictedPaths;
    }

    private bool RemoveCore(string normalizedPath)
    {
        if (!_entries.Remove(normalizedPath, out var entry))
        {
            return false;
        }

        _leastRecentlyUsedPaths.Remove(entry.AccessNode);
        return true;
    }

    private sealed record CacheEntry(
        string ContentHash,
        CompilationResult Result,
        LinkedListNode<string> AccessNode);
}
