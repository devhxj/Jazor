using System.Diagnostics.CodeAnalysis;

namespace Jazor.VueHost.DevServer;

internal sealed class CompilationCache
{
    private readonly Dictionary<string, CacheEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();

    public bool TryGet(string absolutePath, string contentHash, [NotNullWhen(true)] out CompilationResult? result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);

        lock (_gate)
        {
            if (_entries.TryGetValue(absolutePath, out var entry) &&
                string.Equals(entry.ContentHash, contentHash, StringComparison.Ordinal))
            {
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

        lock (_gate)
        {
            if (_entries.TryGetValue(absolutePath, out var entry))
            {
                result = entry.Result;
                return true;
            }
        }

        result = null;
        return false;
    }

    public void Set(string absolutePath, string contentHash, CompilationResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);
        ArgumentNullException.ThrowIfNull(result);

        lock (_gate)
        {
            _entries[absolutePath] = new CacheEntry(contentHash, result);
        }
    }

    public void Invalidate(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);

        lock (_gate)
        {
            _entries.Remove(absolutePath);
        }
    }

    public void InvalidateAll()
    {
        lock (_gate)
        {
            _entries.Clear();
        }
    }

    public IReadOnlyList<string> GetPaths()
    {
        lock (_gate)
        {
            return _entries.Keys.ToArray();
        }
    }

    private sealed record CacheEntry(string ContentHash, CompilationResult Result);
}
