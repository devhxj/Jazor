namespace Jolt.DevServer;

internal sealed class DevServerFileSnapshotPoller : IAsyncDisposable
{
    private readonly string _rootDirectory;
    private readonly TimeSpan _pollInterval;
    private readonly Action<IReadOnlyList<string>> _onChangedPaths;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _pollTask;

    public DevServerFileSnapshotPoller(
        string rootDirectory,
        TimeSpan pollInterval,
        Action<IReadOnlyList<string>> onChangedPaths)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        if (pollInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollInterval));
        }

        _rootDirectory = Path.GetFullPath(rootDirectory);
        _pollInterval = pollInterval;
        _onChangedPaths = onChangedPaths ?? throw new ArgumentNullException(nameof(onChangedPaths));
    }

    public void Start()
    {
        if (_pollTask is not null)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _pollTask = PollAsync(_cancellationTokenSource.Token);
    }

    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        if (_pollTask is not null)
        {
            try
            {
                await _pollTask;
            }
            catch (OperationCanceledException)
            {
            }

            _pollTask = null;
        }
    }

    internal static IReadOnlyDictionary<string, FileSnapshotEntry> CaptureSnapshot(string rootDirectory)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var snapshot = new Dictionary<string, FileSnapshotEntry>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(fullRootDirectory))
        {
            return snapshot;
        }

        foreach (var filePath in EnumerateObservedFiles(fullRootDirectory))
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                snapshot[filePath] = new FileSnapshotEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return snapshot;
    }

    internal static IReadOnlyList<string> GetChangedPaths(
        IReadOnlyDictionary<string, FileSnapshotEntry> previousSnapshot,
        IReadOnlyDictionary<string, FileSnapshotEntry> currentSnapshot)
    {
        var changedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (path, previousEntry) in previousSnapshot)
        {
            if (!currentSnapshot.TryGetValue(path, out var currentEntry)
                || !previousEntry.Equals(currentEntry))
            {
                changedPaths.Add(path);
            }
        }

        foreach (var path in currentSnapshot.Keys)
        {
            if (!previousSnapshot.ContainsKey(path))
            {
                changedPaths.Add(path);
            }
        }

        return changedPaths
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        var previousSnapshot = CaptureSnapshot(_rootDirectory);

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_pollInterval, cancellationToken);
            var currentSnapshot = CaptureSnapshot(_rootDirectory);
            var changedPaths = GetChangedPaths(previousSnapshot, currentSnapshot);
            previousSnapshot = currentSnapshot;

            if (changedPaths.Count > 0)
            {
                _onChangedPaths(changedPaths);
            }
        }
    }

    private static IEnumerable<string> EnumerateObservedFiles(string rootDirectory)
    {
        var pendingDirectories = new Stack<string>();
        var visitedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        pendingDirectories.Push(rootDirectory);

        while (pendingDirectories.Count > 0)
        {
            var currentDirectory = Path.GetFullPath(pendingDirectories.Pop());
            if (!visitedDirectories.Add(currentDirectory))
            {
                continue;
            }

            // Directory.Enumerate* is lazy and can start failing after enumeration begins if files disappear.
            foreach (var childDirectory in SafeEnumerate(() => Directory.EnumerateDirectories(currentDirectory)))
            {
                if (!ShouldDescendIntoDirectory(rootDirectory, childDirectory))
                {
                    continue;
                }

                pendingDirectories.Push(childDirectory);
            }

            foreach (var filePath in SafeEnumerate(() => Directory.EnumerateFiles(currentDirectory)))
            {
                if (DevServerFileWatchFilter.ShouldObserve(rootDirectory, filePath))
                {
                    yield return filePath;
                }
            }
        }
    }

    private static IEnumerable<string> SafeEnumerate(Func<IEnumerable<string>> factory)
    {
        IEnumerator<string>? enumerator = null;
        try
        {
            enumerator = factory().GetEnumerator();
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }

        using (enumerator)
        {
            while (true)
            {
                string current;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        yield break;
                    }

                    current = enumerator.Current;
                }
                catch (DirectoryNotFoundException)
                {
                    yield break;
                }
                catch (IOException)
                {
                    yield break;
                }
                catch (UnauthorizedAccessException)
                {
                    yield break;
                }

                yield return current;
            }
        }
    }

    private static bool ShouldDescendIntoDirectory(string rootDirectory, string directoryPath)
    {
        if (DevServerFileWatchFilter.IsIgnoredDirectoryName(Path.GetFileName(directoryPath)))
        {
            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(directoryPath);
            var relativePath = Path.GetRelativePath(rootDirectory, fullPath);
            if (string.Equals(relativePath, "..", StringComparison.Ordinal)
                || relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                || Path.IsPathRooted(relativePath))
            {
                return false;
            }

            var attributes = File.GetAttributes(fullPath);
            return (attributes & FileAttributes.ReparsePoint) == 0;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    internal readonly record struct FileSnapshotEntry(long Length, long LastWriteTimeUtcTicks);
}
